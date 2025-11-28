using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models;
using FBMS_CumulusFlights.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using FBMS_CumulusFlights.ViewModels;
using FBMS_CumulusFlights.ViewModel;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace FBMS_CumulusFlights.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager; // Initialize UserManager
        }

        public async Task<IActionResult> Index()
        {
            var flightDealsQuery = _context.Flights
                .Where(f => f.ApprovalStatus == ApprovalStatus.Approved &&
                            f.OperationalStatus == OperationalStatus.InProgress &&
                            !f.IsDeleted);

            var today = DateTime.Today;
            flightDealsQuery = flightDealsQuery.Where(f => f.DepartureDate >= today);

            var flightDeals = await flightDealsQuery
                .OrderBy(f => f.DepartureDate)
                .ThenBy(f => f.Price)
                .Take(8)
                .ToListAsync();

            var flightDealViewModels = flightDeals.Select(f => new FlightDealViewModel
            {
                Destination = $"{f.OriginCity} ({f.OriginAirportCode}) to {f.DestinationCity} ({f.DestinationAirportCode})",
                TravelDates = $"{f.DepartureDate:dd MMM yyyy}", // Adjust based on your data structure
                Price = $"Php. {f.Price:N2}",
                FlightId = f.FlightId // Include FlightId for booking
            }).ToList();

            return View(flightDealViewModels);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> UserDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var reservations = await _context.Reservations
                .Include(r => r.Flight)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            // Update status based on flight departure date
            foreach (var reservation in reservations)
            {
                if (reservation.Flight.DepartureDate < DateTime.Now)
                {
                    reservation.Status = ReservationStatusEnum.Completed; // Assuming Status is of type ReservationStatusEnum
                }
            }

            // Order by status: Confirmed first, then Completed
            var orderedReservations = reservations
                .OrderBy(r => r.Status == ReservationStatusEnum.Confirmed ? 0 : 1) // Confirmed first
                .ThenBy(r => r.Flight.DepartureDate) // Then by departure date
                .ToList();

            var reservationViewModels = orderedReservations.Select(r => new ReservationViewModel
            {
                ReservationId = r.ReservationId,
                FlightRoute = $"{(r.Flight.OriginCity ?? r.Flight.OriginAirportCode)} → {(r.Flight.DestinationCity ?? r.Flight.DestinationAirportCode)}",
                FlightNumber = r.Flight.FlightNumber,
                DepartureDate = r.Flight.DepartureDate,
                Status = r.Status.ToString(),
                TotalAmount = r.TotalAmount // Populate the total amount
            }).ToList();

            var nextTrip = reservationViewModels
                .Where(r => r.DepartureDate >= DateTime.Now)
                .OrderBy(r => r.DepartureDate)
                .FirstOrDefault();

            var viewModel = new UserDashboardViewModel
            {
                FirstName = user.FirstName,
                Reservations = reservationViewModels,
                NextTrip = nextTrip
            };

            return View(viewModel);
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
    }
}
