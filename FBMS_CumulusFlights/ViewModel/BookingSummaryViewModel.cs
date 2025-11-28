using Cumulus_Flights.Controllers;
using FBMS_CumulusFlights.Models;
using System.Collections.Generic;

namespace FBMS_CumulusFlights.ViewModels
{
    public class BookingSummaryViewModel
    {
        public Flight Flight { get; set; }

        public List<PassengerViewModel> Passengers { get; set; }


    }
}
