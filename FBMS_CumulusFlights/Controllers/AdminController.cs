using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FBMS_CumulusFlights.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models.Enums;
using FBMS_CumulusFlights.ViewModels;
using iText.Commons.Actions.Contexts;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Cumulus_Flights.Models.Enums;
using FBMS_CumulusFlights.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;


namespace FBMS_CumulusFlights.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Updated Dashboard Action
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.UtcNow.Date;
            var threeDaysAhead = today.AddDays(3);
            var threeDaysBefore = today.AddDays(-3);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Fetch upcoming flights
            var upcomingFlights = await _context.Flights
                .Where(f => (f.ApprovalStatus == ApprovalStatus.Approved || f.ApprovalStatus == ApprovalStatus.Closed)
                            && f.DepartureDate >= today && f.DepartureDate <= threeDaysAhead)
                .ToListAsync();

            // Fetch recent bookings within the current month
            var recentBookings = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Flight)
                .Where(r => r.ReservationDate >= startOfMonth && r.ReservationDate <= endOfMonth)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            // Metrics
            var totalBookings = await _context.Reservations.CountAsync();
            var upcomingFlightsCount = upcomingFlights.Count;
            var totalRevenue = await _context.Reservations.SumAsync(r => r.TotalAmount);
            var pendingFlights = await _context.Flights.CountAsync(f => f.ApprovalStatus == ApprovalStatus.Pending && f.OperationalStatus == OperationalStatus.InProgress);

            var confirmedBookings = await _context.Reservations
                .CountAsync(r => r.Status == ReservationStatusEnum.Confirmed && r.ReservationDate >= startOfMonth && r.ReservationDate <= endOfMonth);
            var pendingBookings = await _context.Reservations
                .CountAsync(r => r.Status == ReservationStatusEnum.Pending && r.ReservationDate >= startOfMonth && r.ReservationDate <= endOfMonth);
            var cancelledBookings = await _context.Reservations
                .CountAsync(r => r.Status == ReservationStatusEnum.Cancelled && r.ReservationDate >= startOfMonth && r.ReservationDate <= endOfMonth);

            // Create the view model
            var viewModel = new AdminDashboardViewModel
            {
                TotalBookings = totalBookings,
                UpcomingFlightsCount = upcomingFlightsCount,
                TotalRevenue = totalRevenue,
                PendingFlights = pendingFlights,
                ConfirmedBookings = confirmedBookings,
                PendingBookings = pendingBookings,
                CancelledBookings = cancelledBookings,
                RecentBookings = recentBookings,
                UpcomingFlights = upcomingFlights
            };

            return View(viewModel);
        }




        public async Task<IActionResult> Report(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string reportType = "bookings")
        {
            // Set default date range (current month)
            var today = DateTime.UtcNow;
            var defaultStartDate = new DateTime(today.Year, today.Month, 1); // First day of the current month
            var defaultEndDate = defaultStartDate.AddMonths(1).AddDays(-1); // Last day of the current month

            // Use provided dates or default to the current month
            var start = startDate ?? defaultStartDate;
            var end = endDate ?? defaultEndDate;

            ViewBag.StartDate = start.Date;
            ViewBag.EndDate = end.Date;
            ViewBag.ReportType = reportType;

            // Calculate metrics
            var totalBookings = await _context.Reservations
                .Where(r => r.ReservationDate >= start && r.ReservationDate <= end)
                .CountAsync();

            var totalFlights = await _context.Flights.CountAsync(); // Adjust as needed for filtering
            var totalPaymentsReceived = await _context.Payments
                .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
                .SumAsync(p => p.PaymentAmount);

            var averageBookingsPerDay = totalBookings > 0 ? totalBookings / (double)(end.Date - start.Date).TotalDays : 0;

            // Generate Report Data for the specified month
            var reportData = await GenerateReportData(start, end);

            var model = new ReportViewModel
            {
                TotalBookings = totalBookings,
                TotalPaymentsReceived = totalPaymentsReceived,
                TotalFlights = totalFlights,
                AverageBookingsPerDay = averageBookingsPerDay,
                ReportData = reportData
            };

            return View(model);
        }

        private async Task<List<ReportData>> GenerateReportData(DateTime startDate, DateTime endDate)
        {
            // Query to get reservations for the specified month
            var reportData = await _context.Reservations
                .Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate)
                .Select(r => new ReportData
                {
                    Date = r.ReservationDate.Date,
                    Bookings = 1, // Each reservation counts as one booking
                    PaymentReceived = r.Payments.Sum(p => p.PaymentAmount) // Sum of payments for this reservation
                })
                .GroupBy(r => r.Date)
                .Select(g => new ReportData
                {
                    Date = g.Key,
                    Bookings = g.Sum(x => x.Bookings),
                    PaymentReceived = g.Sum(x => x.PaymentReceived)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            return reportData;
        }

        public async Task<IActionResult> ExportToPdf(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Set the date range for the report
            var today = DateTime.UtcNow;
            var defaultStartDate = new DateTime(today.Year, today.Month, 1);
            var defaultEndDate = defaultStartDate.AddMonths(1).AddDays(-1);

            var start = startDate ?? defaultStartDate;
            var end = endDate ?? defaultEndDate;

            // Generate the report data
            var reportData = await GenerateReportData(start, end);

            // Create a PDF document using iTextSharp
            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);

                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Flight Booking Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);

                // Add date range
                var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                Paragraph dates = new Paragraph(
                    $"From: {start.ToShortDateString()} To: {end.ToShortDateString()}", dateFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(dates);

                // Add spacing
                document.Add(new Paragraph(" "));

                // Create table
                PdfPTable table = new PdfPTable(3);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 1.5f, 2f });

                // Add headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                table.AddCell(new Phrase("Date", headerFont));
                table.AddCell(new Phrase("Bookings", headerFont));
                table.AddCell(new Phrase("Payment Received", headerFont));

                // Initialize totals
                int totalBookings = 0;
                decimal totalPaymentsReceived = 0;

                // Add data rows
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                foreach (var item in reportData)
                {
                    table.AddCell(new Phrase(item.Date.ToString("d"), cellFont));
                    table.AddCell(new Phrase(item.Bookings.ToString(), cellFont));
                    table.AddCell(new Phrase($"₱{item.PaymentReceived:N2}", cellFont));

                    // Update totals
                    totalBookings += item.Bookings;
                    totalPaymentsReceived += item.PaymentReceived;
                }

                // Add a summary row
                var summaryFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                table.AddCell(new Phrase("Total", summaryFont));
                table.AddCell(new Phrase(totalBookings.ToString(), summaryFont));
                table.AddCell(new Phrase($"₱{totalPaymentsReceived:N2}", summaryFont));

                document.Add(table);
                document.Close();

                // Return the PDF as a file
                var fileName = $"FlightBookingReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                return File(stream.ToArray(), "application/pdf", fileName);
            }
        }


        public async Task<IActionResult> ManageUsers(string searchTerm, string status, string userType)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchTerm) ||
                                                    u.Email.Contains(searchTerm) ||
                                                    u.PhoneNumber.Contains(searchTerm));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Active")
                {
                    usersQuery = usersQuery.Where(u => u.Status == StatusEnum.Active);
                }
                else if (status == "Inactive")
                {
                    usersQuery = usersQuery.Where(u => u.Status == StatusEnum.Inactive);
                }
            }

            // Filter by user type
            if (!string.IsNullOrEmpty(userType) && Enum.TryParse<UserTypeEnum>(userType, out var parsedUserType))
            {
                usersQuery = usersQuery.Where(u => u.UserType == parsedUserType);
            }

            var users = await usersQuery.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> GetActivityLogs(string userId)
        {
            var logs = await _context.UserActivityLogs
                                     .Where(log => log.UserId == userId)
                                     .OrderByDescending(log => log.LoginTime)
                                     .Select(log => new
                                     {
                                         loginTime = log.LoginTime,
                                         logoutTime = log.LogoutTime,
                                         ipAddress = log.IpAddress,
                                         isSuccessful = log.IsSuccessful
                                     })
                                     .ToListAsync();
            return Ok(logs);
        }
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Map ApplicationUser to ViewModel
            var vm = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FormattedDateOfBirth = user.DateOfBirth.ToString("d"), // Short date format
                Gender = user.Gender,
                Nationality = user.Nationality,
                Address = user.Address,
                UserType = user.UserType.ToString(),
                Status = user.Status.ToString(),
                DateJoined = user.DateJoined.ToString("g") // General date/time
            };

            return PartialView("_UserDetailsPartial", vm);
        }


        [Authorize(Policy = "SuperAdminOnly")]
        [HttpGet]
        [Route("Admin/AssignRole")]
        public async Task<IActionResult> AssignRole(string searchTerm, string userType, string status)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchTerm) ||
                                                    u.Email.Contains(searchTerm) ||
                                                    u.PhoneNumber.Contains(searchTerm));
            }

            // Filter by user type
            if (!string.IsNullOrEmpty(userType) && Enum.TryParse<UserTypeEnum>(userType, out var parsedUserType))
            {
                usersQuery = usersQuery.Where(u => u.UserType == parsedUserType);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatusEnum>(status, out var parsedStatus))
            {
                usersQuery = usersQuery.Where(u => u.Status == parsedStatus);
            }

            // Exclude SuperAdmin and filter only active users
            usersQuery = usersQuery.Where(u => u.UserType != UserTypeEnum.SuperAdmin);

            var users = await usersQuery.ToListAsync();
            return View(users);
        }


        [HttpPost]
        [Route("Admin/RevokeAccess")]
        public async Task<IActionResult> RevokeAccess(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.UserType = UserTypeEnum.NormalUser;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [Route("Admin/GrantAccess")]
        public async Task<IActionResult> GrantAccess(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.UserType = UserTypeEnum.Admin;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false });
        }

    }
}
