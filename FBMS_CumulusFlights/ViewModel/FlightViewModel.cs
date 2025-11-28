using System;
using System.ComponentModel.DataAnnotations;

namespace FBMS_CumulusFlights.ViewModel
{
    public class FlightViewModel
    {
        [Required]
        [MaxLength(255)]
        public string OriginCity { get; set; }

        [Required]
        [MaxLength(10)]
        public string OriginAirportCode { get; set; }

        [MaxLength(255)]
        public string OriginAirportName { get; set; }

        [Required]
        [MaxLength(255)]
        public string DestinationCity { get; set; }

        [Required]
        [MaxLength(10)]
        public string DestinationAirportCode { get; set; }

        [MaxLength(255)]
        public string DestinationAirportName { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [Required]
        public decimal DistanceKm { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }
    }
}