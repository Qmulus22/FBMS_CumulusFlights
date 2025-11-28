using System;
using System.ComponentModel.DataAnnotations;
using FBMS_CumulusFlights.Models.Enums;

namespace FBMS_CumulusFlights.Models
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

        [MaxLength(50)]
        public string? FlightNumber { get; set; }

        // Origin Information
        [MaxLength(255)]
        public string? OriginCity { get; set; }

        [Required]
        [MaxLength(10)]
        public string OriginAirportCode { get; set; }

        [MaxLength(255)]
        public string? OriginAirportName { get; set; }

        // Destination Information
        [MaxLength(255)]
        public string? DestinationCity { get; set; }

        [Required]
        [MaxLength(10)]
        public string DestinationAirportCode { get; set; }

        [MaxLength(255)]
        public string? DestinationAirportName { get; set; }

        // Flight Timing
        [Required]
        public DateTime DepartureDate { get; set; }

        // Flight Details
        public decimal DistanceKm { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public double FlightDuration { get; set; }  // Duration in hours

        // Default FlightType to OneWay
        [Required]
        public FlightType FlightType { get; set; } = FlightType.OneWay;

        // Default SourceType to API
        [Required]
        public FlightSource SourceType { get; set; } = FlightSource.API;

        // Status Fields
        [Required]
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        [Required]
        public OperationalStatus OperationalStatus { get; set; } = OperationalStatus.InProgress;

        // System Fields
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
