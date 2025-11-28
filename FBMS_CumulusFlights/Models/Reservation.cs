using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FBMS_CumulusFlights.Models.Enums;

namespace FBMS_CumulusFlights.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int FlightId { get; set; }
        [ForeignKey("FlightId")]
        public Flight Flight { get; set; }

        [Required]
    

        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(10)]
        public ReservationStatusEnum Status { get; set; } = ReservationStatusEnum.Pending;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BaggageWeight { get; set; } = 0.00m;


        [Column(TypeName = "decimal(10, 2)")]
        public decimal AdditionalBaggageFee { get; set; } = 0.00m;


        [Column(TypeName = "decimal(10, 2)")]
        public decimal SeatAdditionalFee { get; set; } = 0.00m;

        // Navigation property for Tickets
        public virtual ICollection<Ticket> Tickets { get; set; }

        // Navigation property for Payments
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
