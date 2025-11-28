using FBMS_CumulusFlights.Models;
using FBMS_CumulusFlights.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FBMS_CumulusFlights.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Traveler> Travelers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            // Store Enums as strings in the database
            builder.Entity<ApplicationUser>()
                .Property(u => u.UserType)
                .HasConversion<string>();

            builder.Entity<ApplicationUser>()
                .Property(u => u.Status)
                .HasConversion<string>();

            // Default value for DateJoined
            builder.Entity<ApplicationUser>()
                .Property(u => u.DateJoined)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure Flight entity
            builder.Entity<Flight>()
                .Property(f => f.ApprovalStatus)
                .HasConversion<string>();

            builder.Entity<Flight>()
                .Property(f => f.OperationalStatus)
                .HasConversion<string>();

            builder.Entity<Flight>()
                .Property(f => f.FlightType)
                .HasConversion<string>();

            builder.Entity<Flight>()
                .Property(f => f.SourceType)
                .HasConversion<string>();

            // Default value for AddedAt
            builder.Entity<Flight>()
                .Property(f => f.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Flight>()
                .Property(f => f.Price)
                .HasColumnType("decimal(10, 2)");

            builder.Entity<Flight>()
                .Property(f => f.DistanceKm)
                .HasColumnType("decimal(10, 2)");

            // Configure Reservation entity
            builder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>();

            builder.Entity<Ticket>()
                .Property(r => r.BaggageType)
                .HasConversion<string>();

            builder.Entity<Ticket>()
                .Property(r => r.SeatClass)
                .HasConversion<string>();

            builder.Entity<Ticket>()
                .Property(r => r.SeatType)
                .HasConversion<string>();

            builder.Entity<Reservation>()
                .Property(r => r.TotalAmount)
                .HasColumnType("decimal(10, 2)");

            builder.Entity<Reservation>()
                .Property(r => r.BaggageWeight)
                .HasColumnType("decimal(10, 2)");

            builder.Entity<Reservation>()
                .Property(r => r.AdditionalBaggageFee)
                .HasColumnType("decimal(10, 2)");

            builder.Entity<Reservation>()
                .Property(r => r.SeatAdditionalFee)
                .HasColumnType("decimal(10, 2)");

            // Configure Payment entity
            builder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();

            builder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasConversion<string>();

            builder.Entity<Payment>()
                .Property(p => p.PaymentAmount)
                .HasColumnType("decimal(10, 2)");

            // Configure Ticket entity
            builder.Entity<Ticket>()
                .HasOne(t => t.Traveler)
                .WithMany(tr => tr.Tickets)
                .HasForeignKey(t => t.TravelerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Ticket>()
                .HasOne(t => t.Payment)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Travelers entity
            builder.Entity<Traveler>()
                .Property(t => t.Gender)
                .HasConversion<string>();

            builder.Entity<Traveler>()
                .Property(t => t.PassengerType)
                .HasConversion<string>();

 
        }
    }
}