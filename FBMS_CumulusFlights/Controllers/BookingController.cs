using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models;
using FBMS_CumulusFlights.Models.Enums;
using FBMS_CumulusFlights.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FBMS_CumulusFlights.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Cumulus_Flights.Controllers
{

    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayPalService _payPalService;


        public BookingController(ApplicationDbContext context, PayPalService payPalService)
        {
            _context = context;
            _payPalService = payPalService; // Initialize PayPalService
         
        }

        public async Task<IActionResult> Index(string from, string to, DateTime departureDate, decimal? maxPrice)
        {
            var flightsQuery = _context.Flights
                .Where(f => f.ApprovalStatus == ApprovalStatus.Approved &&
                            f.OperationalStatus == OperationalStatus.InProgress &&
                            !f.IsDeleted);

            if (!string.IsNullOrEmpty(from))
                flightsQuery = flightsQuery.Where(f => f.OriginCity.Contains(from) ||
                                                     f.OriginAirportCode.Contains(from));

            if (!string.IsNullOrEmpty(to))
                flightsQuery = flightsQuery.Where(f => f.DestinationCity.Contains(to) ||
                                                     f.DestinationAirportCode.Contains(to));

            if (departureDate != default)
                flightsQuery = flightsQuery.Where(f => f.DepartureDate.Date == departureDate.Date);

            if (maxPrice.HasValue)
                flightsQuery = flightsQuery.Where(f => f.Price <= maxPrice.Value);

            var flights = await flightsQuery.OrderBy(f => f.DepartureDate)
                                           .ThenBy(f => f.Price)
                                           .ToListAsync();
            return View(flights);
        }

        public async Task<IActionResult> BookingSummary(int flightId)
        {
            var flight = await _context.Flights.FirstOrDefaultAsync(f => f.FlightId == flightId);
            return flight == null ? NotFound() : View(new BookingSummaryViewModel
            {
                Flight = flight,
                Passengers = new List<PassengerViewModel> { new() { Index = 1 } }
            });
        }




        [HttpPost]
        public IActionResult ProcessBooking(int FlightId, decimal Price, List<PassengerViewModel> passengers)
        {
            var flight = _context.Flights.Find(FlightId);
            if (flight == null) return NotFound();

            foreach (var p in passengers)
            {
                p.BaggageFee = p.BaggageWeight * GetBaggageRate(p.BaggageType);
                p.SeatFee = GetSeatFee(p.SeatClass, p.SeatType);
            }

            var totalAmount = flight.Price * passengers.Count + passengers.Sum(p => p.BaggageFee + p.SeatFee);
            return View("Checkout", new CheckoutViewModel
            {
                Flight = flight,
                TotalAmount = totalAmount,
                Passengers = passengers
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreatePayPalOrder(decimal totalAmount)
        {
            try
            {
                var orderId = await _payPalService.CreateOrder(totalAmount);
                return Json(new { id = orderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize] // Add authorization
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout([FromBody] CheckoutViewModel model)
        {
            const string logFilePath = "booking_log.txt";
            const decimal exchangeRateUSDToPHP = 55.0m;


            try
            {
                decimal totalAmountPHP = model.TotalAmount * exchangeRateUSDToPHP;
                // Get current user ID properly
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    LogToFile(logFilePath, "User Error", "User not authenticated");
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verify user exists in database
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    LogToFile(logFilePath, "User Error", $"User ID not found: {userId}");
                    return Json(new { success = false, message = "User account not found" });
                }

                // Rest of your validation
                if (model?.Flight == null || model.Passengers == null || !model.Passengers.Any())
                {
                    LogToFile(logFilePath, "Validation Failed",
                        $"Model null: {model == null}, Flight null: {model?.Flight == null}, Passengers: {model?.Passengers?.Count ?? 0}");
                    return Json(new { success = false, message = "Invalid booking data" });
                }



                var flight = await _context.Flights.FindAsync(model.Flight.FlightId);
                if (flight == null)
                {
                    LogToFile(logFilePath, "Flight Not Found", $"ID: {model.Flight.FlightId}");
                    return Json(new { success = false, message = "Flight not found" });
                }

                var ticketCount = await _context.Tickets.CountAsync(t => t.FlightId == flight.FlightId);
                if (ticketCount >= 180)
                {
                    flight.ApprovalStatus = ApprovalStatus.Closed; // Update approval status
                    _context.Flights.Update(flight);
                    await _context.SaveChangesAsync();
                    return Json(new { success = false, message = "Flight is fully booked." });
                }
                // Capture the payment using PayPalService
                var orderId = model.PayPalOrderId; // Ensure this is passed from the front end
            


                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create Reservation with validated user ID
                    var reservation = new Reservation
                    {
                        UserId = userId, // Use the validated user ID
                        FlightId = flight.FlightId,
                        ReservationDate = DateTime.UtcNow,
                        TotalAmount = totalAmountPHP,
                        BaggageWeight = model.Passengers.Sum(p => p.BaggageWeight),
                        AdditionalBaggageFee = model.Passengers.Sum(p => p.BaggageFee),
                        SeatAdditionalFee = model.Passengers.Sum(p => p.SeatFee),
                        Status = ReservationStatusEnum.Confirmed
                    };

                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();


        

                    // Create Payment record
                    var payment = new Payment
                    {
                        ReservationId = reservation.ReservationId,
                        PaymentMethod = PaymentMethodEnum.PayPal,
                        PaymentAmount = totalAmountPHP,
                        PaymentDate = DateTime.UtcNow,
                        Status = "Paid",
                        TransactionReference = orderId // Use the PayPal order ID as the transaction reference
                    };
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    // Process Passengers
                    foreach (var passenger in model.Passengers)
                    {
                        var traveler = await _context.Travelers
                            .FirstOrDefaultAsync(t =>
                                t.PassportNumber == passenger.PassportNumber &&
                                t.PassportCountry == passenger.PassportCountry &&
                                t.DateOfBirth == passenger.DateOfBirth) ?? new Traveler
                                {
                                    FirstName = passenger.FirstName,
                                    LastName = passenger.LastName,
                                    PassportNumber = passenger.PassportNumber,
                                    PassportCountry = passenger.PassportCountry,
                                    DateOfBirth = passenger.DateOfBirth,
                                    Gender = passenger.Gender,
                                    PassengerType = passenger.PassengerClassification
                                };

                        if (traveler.TravelerId == 0)
                        {
                            _context.Travelers.Add(traveler);
                            await _context.SaveChangesAsync();
                        }

                        _context.Tickets.Add(new Ticket
                        {
                            TravelerId = traveler.TravelerId,
                            PaymentId = payment.PaymentId,
                            ReservationId = reservation.ReservationId,
                            FlightId = flight.FlightId,
                            TicketNumber = $"{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 15),
                            SeatNumber = $"{passenger.SeatClass.ToString()[0]}-{new Random().Next(1, 50)}{passenger.SeatType.ToString()[0]}",
                            SeatClass = passenger.SeatClass, // Add this
                            SeatType = passenger.SeatType,   // Add this
                            BaggageType = passenger.BaggageType, // Add this
                            IssuedAt = DateTime.UtcNow,
                            Status = "Confirmed"
                        });
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new
                    {
                        success = true,
                        message = "Booking confirmed!",
                        reservationId = reservation.ReservationId
                    });



                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    LogToFile(logFilePath, "Transaction Error", ex.ToString());
                    return Json(new { success = false, message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                LogToFile(logFilePath, "General Error", ex.ToString());
                return Json(new { success = false, message = "Booking failed. Please try again." });
            }
        }
        private void LogToFile(string filePath, string title, string content)
        {
            using (StreamWriter writer = System.IO.File.AppendText(filePath))
            {
                writer.WriteLine($"[{DateTime.UtcNow:u}] {title}");
                writer.WriteLine(content);
                writer.WriteLine(new string('-', 50));
            }
        }

        public async Task<IActionResult> DownloadCombinedPdf(int id)
        {
            try
            {
                // Get all related data
                var reservation = await _context.Reservations
                    .Include(r => r.Flight)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.ReservationId == id);

                if (reservation == null)
                {
                    return NotFound();
                }

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.ReservationId == id);

                var tickets = await _context.Tickets
                    .Include(t => t.Traveler)
                    .Where(t => t.ReservationId == id)
                    .ToListAsync();

                // Generate PDF
                var pdfBytes = GenerateReceiptPdf(reservation, payment, tickets);
                return File(pdfBytes, "application/pdf", $"Receipt-{id}.pdf");
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, "Error generating PDF: " + ex.Message);
            }
        }

        private byte[] GenerateReceiptPdf(Reservation reservation, Payment payment, List<Ticket> tickets)
        {
            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 40, 40, 40, 30);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add styling
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                // Add logo
                try
                {
                    var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "CompanyLogo.png");
                    if (System.IO.File.Exists(logoPath))
                    {
                        var logo = Image.GetInstance(logoPath);
                        logo.ScaleToFit(100, 60);
                        logo.Alignment = Image.ALIGN_CENTER;
                        document.Add(logo);
                    }
                }
                catch { /* Handle missing logo gracefully */ }

                // Header
                var header = new Paragraph("FLIGHT BOOKING RECEIPT", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(header);

                // Company Info
                var companyInfo = new PdfPTable(2);
                companyInfo.WidthPercentage = 100;
                companyInfo.SetWidths(new float[] { 1f, 1f });
                companyInfo.AddCell(new Phrase("Cumulus Flights", boldFont));
                companyInfo.AddCell(new Phrase($"Date: {DateTime.Now:dd MMM yyyy HH:mm}", normalFont));
                companyInfo.AddCell(new Phrase("University of Mindanao", normalFont));
                companyInfo.AddCell(new Phrase($"Receipt #: {reservation.ReservationId}", normalFont));
                companyInfo.AddCell(new Phrase("Davao, Philippines", normalFont));
                companyInfo.AddCell(new Phrase($"Transaction ID: {payment.TransactionReference}", normalFont));
                document.Add(companyInfo);

                // Add space
                document.Add(new Paragraph(" "));

                // Flight Information
                var flightHeader = new Paragraph("Flight Details", headerFont);
                document.Add(flightHeader);

                var flightTable = new PdfPTable(4);
                flightTable.WidthPercentage = 100;
                flightTable.SetWidths(new float[] { 2f, 3f, 2f, 3f });
                flightTable.AddCell(CreateReceiptCell("Flight Number:", boldFont));
                flightTable.AddCell(CreateReceiptCell(reservation.Flight.FlightNumber, normalFont));
                flightTable.AddCell(CreateReceiptCell("Departure Date:", boldFont));
                flightTable.AddCell(CreateReceiptCell(reservation.Flight.DepartureDate.ToString("dd MMM yyyy HH:mm"), normalFont));
                flightTable.AddCell(CreateReceiptCell("From:", boldFont));
                flightTable.AddCell(CreateReceiptCell($"{reservation.Flight.OriginCity} ({reservation.Flight.OriginAirportCode})", normalFont));
                flightTable.AddCell(CreateReceiptCell("To:", boldFont));
                flightTable.AddCell(CreateReceiptCell($"{reservation.Flight.DestinationCity} ({reservation.Flight.DestinationAirportCode})", normalFont));
                document.Add(flightTable);

                // Passengers
                document.Add(new Paragraph("Passengers", headerFont));
                var passengerTable = new PdfPTable(new float[] { 3f, 3f, 2f, 2f, 2f });
                passengerTable.WidthPercentage = 100;
                passengerTable.SetWidths(new float[] { 2f, 2f, 1f, 1f, 1f });
                passengerTable.AddCell(CreateReceiptCell("Name", boldFont));
                passengerTable.AddCell(CreateReceiptCell("Passport", boldFont));
                passengerTable.AddCell(CreateReceiptCell("Seat", boldFont));
                passengerTable.AddCell(CreateReceiptCell("Baggage", boldFont));
                passengerTable.AddCell(CreateReceiptCell("Class", boldFont));

                foreach (var ticket in tickets)
                {
                    passengerTable.AddCell(CreateReceiptCell($"{ticket.Traveler.FirstName} {ticket.Traveler.LastName}", normalFont));
                    passengerTable.AddCell(CreateReceiptCell($"{ticket.Traveler.PassportNumber}", normalFont));
                    passengerTable.AddCell(CreateReceiptCell(ticket.SeatNumber, normalFont));
                    passengerTable.AddCell(CreateReceiptCell($"{ticket.BaggageType}", normalFont));
                    passengerTable.AddCell(CreateReceiptCell($"{ticket.SeatClass}", normalFont));
                }
                document.Add(passengerTable);

                // Payment Summary
                document.Add(new Paragraph("Payment Summary", headerFont));
                var paymentTable = new PdfPTable(2);
                paymentTable.WidthPercentage = 100;
                paymentTable.SetWidths(new float[] { 3f, 2f });

                paymentTable.AddCell(CreateReceiptCell("Base Fare:", boldFont));
                paymentTable.AddCell(CreateReceiptCell($"₱{reservation.Flight.Price * tickets.Count:N2}", normalFont));
                paymentTable.AddCell(CreateReceiptCell("Baggage Fees:", boldFont));
                paymentTable.AddCell(CreateReceiptCell($"₱{reservation.AdditionalBaggageFee:N2}", normalFont));
                paymentTable.AddCell(CreateReceiptCell("Seat Fees:", boldFont));
                paymentTable.AddCell(CreateReceiptCell($"₱{reservation.SeatAdditionalFee:N2}", normalFont));
                paymentTable.AddCell(CreateReceiptCell("Total Amount:", boldFont));
                paymentTable.AddCell(CreateReceiptCell($"₱{reservation.TotalAmount:N2}", normalFont));

                document.Add(paymentTable);

                document.NewPage();
                CreateBoardingPass(document, tickets, reservation);

                // Footer
                var footer = new Paragraph("Thank you for choosing Cumulus Flights!\nSafe travels!", normalFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20
                };
                document.Add(footer);

                document.Close();
                return ms.ToArray();
            }
        }

        // Helper method to create a cell with alignment and border for the receipt
        private PdfPCell CreateReceiptCell(string text, Font font, int alignment = Element.ALIGN_LEFT)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 5,
                HorizontalAlignment = alignment,
                BackgroundColor = new BaseColor(255, 255, 255), // White background
                Border = Rectangle.BOX // Add border to each cell
            };
            return cell;
        }


        private void CreateBoardingPass(Document document, List<Ticket> tickets, Reservation reservation)
        {
            var passFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, Font.UNDERLINE);

            foreach (var ticket in tickets)
            {
                // Create a wrapper table to add a border around the entire boarding pass
                var wrapperTable = new PdfPTable(1);
                wrapperTable.WidthPercentage = 100;
                var wrapperCell = new PdfPCell()
                {
                    Border = Rectangle.BOX, // Add border around the cell
                    BorderWidth = 1,
                    Padding = 0
                };

                // Create main table for the boarding pass
                var boardingPassTable = new PdfPTable(2); // Two columns for layout
                boardingPassTable.WidthPercentage = 100;
                boardingPassTable.SetWidths(new float[] { 1f, 1f });

                // Blue Header Section
                var headerCell = new PdfPCell(new Phrase("BOARDING PASS", headerFont))
                {
                    Colspan = 2,
                    BackgroundColor = new BaseColor(0, 112, 192), // Blue color
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Border = Rectangle.NO_BORDER // Remove border for header cell
                };
                boardingPassTable.AddCell(headerCell);

                // Passenger Information
                boardingPassTable.AddCell(CreateCell($"Passenger: {ticket.Traveler.FirstName} {ticket.Traveler.LastName}", boldFont));
                boardingPassTable.AddCell(CreateCell($"Flight: {reservation.Flight.FlightNumber}", boldFont));

                boardingPassTable.AddCell(CreateCell($"From: {reservation.Flight.OriginCity} ({reservation.Flight.OriginAirportCode})", boldFont));
                boardingPassTable.AddCell(CreateCell($"To: {reservation.Flight.DestinationCity} ({reservation.Flight.DestinationAirportCode})", boldFont));

                boardingPassTable.AddCell(CreateCell($"Departure Date: {reservation.Flight.DepartureDate.ToString("dd MMM yyyy HH:mm")}", boldFont));
                boardingPassTable.AddCell(CreateCell($"Seat: {ticket.SeatNumber}", boldFont));

                boardingPassTable.AddCell(CreateCell($"Class/Type: {ticket.SeatClass} / {ticket.SeatType}", boldFont));

                // QR Code Section at the bottom right
                try
                {
                    var qrCodePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "QmulusGitHubProfileQR.jpeg");
                    if (System.IO.File.Exists(qrCodePath))
                    {
                        var qrCodeImage = Image.GetInstance(qrCodePath);
                        qrCodeImage.ScaleToFit(80, 80);
                        var qrCell = new PdfPCell(qrCodeImage)
                        {
                            Border = Rectangle.NO_BORDER,
                            Padding = 5,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        boardingPassTable.AddCell(qrCell);
                    }
                    else
                    {
                        boardingPassTable.AddCell(CreateCell("[QR Code Missing]", passFont, Element.ALIGN_RIGHT));
                    }
                }
                catch
                {
                    boardingPassTable.AddCell(CreateCell("[QR Code Error]", passFont, Element.ALIGN_RIGHT));
                }

                // Add the boarding pass table to the wrapper cell
                wrapperCell.AddElement(boardingPassTable);
                wrapperTable.AddCell(wrapperCell);

                document.Add(wrapperTable);
                document.Add(new Paragraph(" "));
            }
        }

        // Helper method to create a cell with alignment
        private PdfPCell CreateCell(string text, Font font, int alignment = Element.ALIGN_LEFT)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Border = Rectangle.NO_BORDER,
                Padding = 5,
                HorizontalAlignment = alignment,
                BackgroundColor = new BaseColor(255, 255, 255) // White background
            };
            return cell;
        }







        private decimal CalculateTotal(Reservation reservation)
        {
            return reservation.Flight.Price * reservation.Tickets.Count()
                   + reservation.AdditionalBaggageFee
                   + reservation.SeatAdditionalFee;
        }


        private decimal GetBaggageRate(BaggageTypeEnum baggageType)
        {
            return baggageType switch
            {
                BaggageTypeEnum.Standard => 100m,
                BaggageTypeEnum.Excess => 200m,
                BaggageTypeEnum.Special => 150m,
                _ => 0m
            };
        }

        private decimal GetSeatFee(SeatClassEnum seatClass, SeatTypeEnum seatType)
        {
            decimal fee = seatClass switch
            {
                SeatClassEnum.Business => 500m,
                SeatClassEnum.FirstClass => 1000m,
                SeatClassEnum.PremiumEconomy => 300m,
                _ => 0m
            };

            return fee + seatType switch
            {
                SeatTypeEnum.Window => 50m,
                SeatTypeEnum.Aisle => 30m,
                _ => 0m
            };
        }
    }
}