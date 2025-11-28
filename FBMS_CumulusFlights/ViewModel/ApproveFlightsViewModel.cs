using FBMS_CumulusFlights.Models;
using System.Collections.Generic;

namespace FBMS_CumulusFlights.ViewModel
{
    public class ApproveFlightsViewModel
    {
        public List<Flight> Flights { get; set; }
        public FlightSearchViewModel SearchModel { get; set; }
    }
}