using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class FlightStatusUpdateService : BackgroundService
{
    private readonly IServiceProvider _services;

    public FlightStatusUpdateService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var now = DateTime.UtcNow;

                var flights = await context.Flights
                    .Where(f => f.ApprovalStatus == ApprovalStatus.Approved && !f.IsDeleted)
                    .ToListAsync();

                foreach (var flight in flights)
                {
                    var timeToDeparture = flight.DepartureDate - now;
                    if (timeToDeparture < TimeSpan.FromHours(1) && timeToDeparture > TimeSpan.Zero)
                    {
                        flight.OperationalStatus = OperationalStatus.Boarding;
                    }
                    else if (timeToDeparture <= TimeSpan.Zero && flight.OperationalStatus != OperationalStatus.Departed)
                    {
                        flight.OperationalStatus = OperationalStatus.Departed;
                    }
                    else if (now >= flight.DepartureDate.AddHours(flight.FlightDuration) && flight.OperationalStatus != OperationalStatus.Arrived)
                    {
                        flight.OperationalStatus = OperationalStatus.Arrived;
                    }
                    else if (now >= flight.DepartureDate.AddHours(flight.FlightDuration).AddMinutes(30) && flight.OperationalStatus != OperationalStatus.Completed)
                    {
                        flight.OperationalStatus = OperationalStatus.Completed;
                    }
                }

                await context.SaveChangesAsync();
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
