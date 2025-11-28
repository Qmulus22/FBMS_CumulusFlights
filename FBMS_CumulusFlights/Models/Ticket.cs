using FBMS_CumulusFlights.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FBMS_CumulusFlights.Models.Enums;

public class Ticket
{
    [Key]
    public int TicketId { get; set; }

    // Remove ReservationId - it can be accessed via Payment.ReservationId

    [Required]
    public int TravelerId { get; set; }
    [ForeignKey("TravelerId")]
    public Traveler Traveler { get; set; }


    [Required]
    public int PaymentId { get; set; }
    [ForeignKey("PaymentId")]
    public Payment Payment { get; set; } // Payment will link to Reservation

    [Required]
    public int FlightId { get; set; } // Add FlightId to Ticket
    [ForeignKey("FlightId")]
    public Flight Flight { get; set; }

    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string SeatNumber { get; set; } // Ensure SeatNumber is a string

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(10)]
    public string Status { get; set; } = "Confirmed";

    [Required]
    public int ReservationId { get; set; } // Add ReservationId to Ticket
    [ForeignKey("ReservationId")]
    public Reservation Reservation { get; set; }

    [Required]
    public BaggageTypeEnum BaggageType { get; set; } = BaggageTypeEnum.Standard;

    [Required]
    public SeatClassEnum SeatClass { get; set; } = SeatClassEnum.Economy;

    [Required]
    public SeatTypeEnum SeatType { get; set; } = SeatTypeEnum.Middle;

}
