using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FBMS_CumulusFlights.Models.Enums;

namespace FBMS_CumulusFlights.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int ReservationId { get; set; }
        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [Required]
        public PaymentMethodEnum PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PaymentAmount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(10)]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(255)]
        public string TransactionReference { get; set; }

        // Navigation property for Tickets
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
