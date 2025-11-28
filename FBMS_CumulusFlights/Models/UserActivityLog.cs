using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cumulus_Flights.Models.Enums;

namespace FBMS_CumulusFlights.Models
{
    [Table("UserActivityLog")]
    public class UserActivityLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } // Use string to match ApplicationUser's Id

        [Column("login_time", TypeName = "datetime")]
        public DateTime? LoginTime { get; set; }

        [Column("logout_time", TypeName = "datetime")]
        public DateTime? LogoutTime { get; set; }

        [Column("ip_address")]
        [StringLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        [Column("is_successful")]
        public bool IsSuccessful { get; set; } // Whether the login was successful

        [ForeignKey("UserId")]
        [InverseProperty("UserActivityLogs")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
