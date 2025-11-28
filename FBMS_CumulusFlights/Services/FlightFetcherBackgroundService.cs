using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using FBMS_CumulusFlights.Data;

namespace FBMS_CumulusFlights.Services
{
    public class FlightFetcherBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FlightFetcherBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var flightDataService = scope.ServiceProvider.GetRequiredService<FlightDataService>();

                        // Call API to fetch and store flights
                        await flightDataService.FetchAndStoreFlights();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching flights: {ex.Message}");
                }

                // Wait 30 minutes before running again
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
    }
}
