using Cumulus_Flights.Controllers;
using FBMS_CumulusFlights.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FBMS_CumulusFlights.ViewModels
{
    public class CheckoutViewModel
    {
        public Flight Flight { get; set; }
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("Passengers")]
        public List<PassengerViewModel> Passengers { get; set; }

        public string PayPalOrderId { get; set; }
    }
}
