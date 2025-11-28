using FBMS_CumulusFlights.Models.Enums;

namespace FBMS_CumulusFlights.ViewModel
{
    public class FlightSearchViewModel
    {
        public string FlightNumber { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime? DepartureDate { get; set; }
        public TimeSpan? DepartureTimeStart { get; set; } // New property for start time
        public TimeSpan? DepartureTimeEnd { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public OperationalStatus? OperationalStatus { get; set; }

        public decimal? MaxPrice { get; set; }

    }
}
