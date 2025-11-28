using Microsoft.AspNetCore.Identity;
using Cumulus_Flights.Models.Enums; // Import Enums
using System;
using System.Collections.Generic;

namespace FBMS_CumulusFlights.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string Address { get; set; }
        public UserTypeEnum UserType { get; set; } = UserTypeEnum.NormalUser;
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
        public StatusEnum Status { get; set; } = StatusEnum.Active;

        // Navigation property for UserActivityLogs
        public virtual ICollection<UserActivityLog> UserActivityLogs { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
