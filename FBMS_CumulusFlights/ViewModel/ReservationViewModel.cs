using System;
using System.Collections.Generic;

namespace FBMS_CumulusFlights.ViewModel
{
    public class ReservationViewModel
    {
        public int ReservationId { get; set; }
        public string FlightRoute { get; set; }
        public string FlightNumber { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
