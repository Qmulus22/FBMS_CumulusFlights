using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FBMS_CumulusFlights.Models;
using System.Collections.Generic;
using System.Linq;
using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models.Enums;

namespace FBMS_CumulusFlights.Controllers
{
    public class AdminReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminViewReservations(string reservationId, string flightId, string travelerName, DateTime? startDate, DateTime? endDate, ReservationStatusEnum? status)
        {
            var query = _context.Reservations
                .Include(r => r.Payments)
                .Include(r => r.Tickets)
                    .ThenInclude(t => t.Traveler)
                .Include(r => r.Flight)
                .Include(r => r.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(reservationId) && int.TryParse(reservationId, out int resId))
            {
                query = query.Where(r => r.ReservationId == resId);
            }

            if (!string.IsNullOrEmpty(flightId) && int.TryParse(flightId, out int flId))
            {
                query = query.Where(r => r.FlightId == flId);
            }

            if (!string.IsNullOrEmpty(travelerName))
            {
                var nameParts = travelerName.Trim().ToLower().Split(' ');
                query = query.Where(r => r.Tickets.Any(t =>
                    t.Traveler.FirstName.ToLower().Contains(nameParts[0]) ||
                    t.Traveler.LastName.ToLower().Contains(nameParts[0]) ||
                    (nameParts.Length > 1 && (
                        t.Traveler.FirstName.ToLower().Contains(nameParts[1]) ||
                        t.Traveler.LastName.ToLower().Contains(nameParts[1])
                    ))
                ));
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.ReservationDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.ReservationDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // Order by ReservationDate descending
            var reservations = query.OrderByDescending(r => r.ReservationDate).ToList();

            return View(reservations);
        }

    }
}