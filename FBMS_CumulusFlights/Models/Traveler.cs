using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FBMS_CumulusFlights.Models.Enums;

namespace FBMS_CumulusFlights.Models
{
    public class Traveler
    {
        [Key]
        public int TravelerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(100)]
        public string PassportNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string PassportCountry { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; }

        [Required]
        public PassengerType PassengerType { get; set; } = PassengerType.Adult; // Default value set to Adult

        // Navigation property for Tickets
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}