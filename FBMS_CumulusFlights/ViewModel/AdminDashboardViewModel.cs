using System;
using System.Collections.Generic;
using FBMS_CumulusFlights.Models;

namespace FBMS_CumulusFlights.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Metrics
        public int TotalBookings { get; set; }
        public int UpcomingFlightsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingRequests { get; set; }

        public int PendingFlights { get; set; }
        // Booking Statistics
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }

        // Recent Bookings
        public List<Reservation> RecentBookings { get; set; }

        // Upcoming Flights
        public List<Flight> UpcomingFlights { get; set; }
    }
}
