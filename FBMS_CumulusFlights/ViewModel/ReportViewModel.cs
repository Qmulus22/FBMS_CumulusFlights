using System;
using System.Collections.Generic;

namespace FBMS_CumulusFlights.ViewModels
{
    public class ReportViewModel
    {
        public int TotalBookings { get; set; }
        public decimal TotalPaymentsReceived { get; set; }  // Renamed property
        public int TotalFlights { get; set; }
        public double AverageBookingsPerDay { get; set; }
        public List<ReportData> ReportData { get; set; }
    }

    public class ReportData
    {
        public DateTime Date { get; set; }
        public int Bookings { get; set; }
        public int TicketsSold { get; set; }  // New property
        public decimal PaymentReceived { get; set; }  // Renamed property
    }
}
