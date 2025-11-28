using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models;
using FBMS_CumulusFlights.Models.Config;
using FBMS_CumulusFlights.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FBMS_CumulusFlights.Services
{
    public class FlightDataService
    {
        private readonly HttpClient _httpClient;
        private readonly TravelPayoutsAPIOptions _apiOptions;
        private readonly RapidAPIOptions _rapidApiOptions;
        private readonly ApplicationDbContext _context;
        private const decimal AverageSpeedKmh = 900m;

        public FlightDataService(
            HttpClient httpClient,
            IOptions<TravelPayoutsAPIOptions> apiOptions,
            IOptions<RapidAPIOptions> rapidApiOptions,
            ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _apiOptions = apiOptions.Value;
            _rapidApiOptions = rapidApiOptions.Value;
            _context = context;
        }

        public async Task FetchAndStoreFlights()
        {
            try
            {
                var url = $"{_apiOptions.BaseUrl}/prices/latest?currency=php&limit=50&token={_apiOptions.ApiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var flightData = JsonConvert.DeserializeObject<FlightApiResponse>(response);

                if (flightData?.Success == true && flightData.Data != null)
                {
                    Console.WriteLine($"Processing {flightData.Data.Count} flights");
                    foreach (var apiFlight in flightData.Data.Where(f => f.Distance > 0))
                    {
                        await ProcessFlight(apiFlight);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Flight fetch error: {ex.Message}");
            }
        }

        private async Task ProcessFlight(FlightData apiFlight)
        {
            try
            {
                var originInfo = await GetAirportInfo(apiFlight.Origin);
                var destinationInfo = await GetAirportInfo(apiFlight.Destination);

                // Process the departure flight
                var departureDate = apiFlight.GetDepartureDate();
                var flightDuration = CalculateFlightDuration(apiFlight.Distance);

                if (!await FlightExists(apiFlight, departureDate))
                {
                    var departureFlight = new Flight
                    {
                        FlightNumber = GenerateFlightNumber(),
                        OriginCity = originInfo.CityName,
                        OriginAirportCode = apiFlight.Origin,
                        OriginAirportName = originInfo.Name,
                        DestinationCity = destinationInfo.CityName,
                        DestinationAirportCode = apiFlight.Destination,
                        DestinationAirportName = destinationInfo.Name,
                        DepartureDate = departureDate,
                        DistanceKm = apiFlight.Distance,
                        Price = apiFlight.Value,
                        FlightDuration = flightDuration,
                        FlightType = FlightType.OneWay,
                        ApprovalStatus = ApprovalStatus.Pending,
                        OperationalStatus = OperationalStatus.InProgress,
                        SourceType = FlightSource.API,
                        AddedAt = DateTime.UtcNow
                    };

                    await SaveFlightToDatabase(departureFlight);
                }

                // Process the return flight if return_date is provided
                if (!string.IsNullOrEmpty(apiFlight.ReturnDateRaw))
                {
                    var returnDepartureDate = apiFlight.GetReturnDepartureDate();

                    if (!await FlightExists(apiFlight, returnDepartureDate, isReturn: true))
                    {
                        var returnFlight = new Flight
                        {
                            FlightNumber = GenerateFlightNumber(),
                            OriginCity = destinationInfo.CityName,
                            OriginAirportCode = apiFlight.Destination,
                            OriginAirportName = destinationInfo.Name,
                            DestinationCity = originInfo.CityName,
                            DestinationAirportCode = apiFlight.Origin,
                            DestinationAirportName = originInfo.Name,
                            DepartureDate = returnDepartureDate,
                            DistanceKm = apiFlight.Distance,
                            Price = apiFlight.Value,
                            FlightDuration = flightDuration,
                            FlightType = FlightType.OneWay,
                            ApprovalStatus = ApprovalStatus.Pending,
                            OperationalStatus = OperationalStatus.InProgress,
                            SourceType = FlightSource.API,
                            AddedAt = DateTime.UtcNow
                        };

                        await SaveFlightToDatabase(returnFlight);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing flight: {ex.Message}");
            }
        }

        private async Task<bool> FlightExists(FlightData apiFlight, DateTime departureDate, bool isReturn = false)
        {
            return await _context.Flights.AnyAsync(f =>
                f.OriginAirportCode == (isReturn ? apiFlight.Destination : apiFlight.Origin) &&
                f.DestinationAirportCode == (isReturn ? apiFlight.Origin : apiFlight.Destination) &&
                f.DepartureDate == departureDate &&
                f.DistanceKm == apiFlight.Distance &&
                f.Price == apiFlight.Value &&
                f.SourceType == FlightSource.API &&
                !f.IsDeleted);
        }

        private async Task SaveFlightToDatabase(Flight flight)
        {
            try
            {
                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Saved flight {flight.FlightNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database save error: {ex.Message}");
            }
        }

        private double CalculateFlightDuration(decimal distance)
        {
            return (double)(distance / AverageSpeedKmh);
        }

        private string GenerateFlightNumber()
        {
            return $"FL{DateTime.UtcNow:yyMMddHHmmss}";
        }

        private async Task<AirportInfo> GetAirportInfo(string iataCode)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_rapidApiOptions.BaseUrl}/{iataCode}/"),
                    Headers =
            {
                { "x-rapidapi-key", _rapidApiOptions.ApiKey },
                { "x-rapidapi-host", _rapidApiOptions.Host }
            }
                };

                var response = await _httpClient.SendAsync(request);

                // Handle API-specific responses
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Airport with IATA {iataCode} not found");
                    return new AirportInfo(iataCode, iataCode);
                }

                response.EnsureSuccessStatusCode();

                var airport = JsonConvert.DeserializeObject<RapidApiAirport>(
                    await response.Content.ReadAsStringAsync());

                return airport != null
                    ? new AirportInfo(airport.Name, airport.City)
                    : new AirportInfo(iataCode, iataCode);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API Error: {ex.StatusCode} - {ex.Message}");
                return new AirportInfo(iataCode, iataCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                return new AirportInfo(iataCode, iataCode);
            }
        }
        public class FlightApiResponse
        {
            public bool Success { get; set; }
            public List<FlightData> Data { get; set; }
        }

        public class FlightData
        {
            [JsonProperty("depart_date")]
            public string DepartDateRaw { get; set; }

            [JsonProperty("return_date")]
            public string ReturnDateRaw { get; set; }

            [JsonProperty("origin")]
            public string Origin { get; set; }

            [JsonProperty("destination")]
            public string Destination { get; set; }

            [JsonProperty("distance")]
            public decimal Distance { get; set; }

            [JsonProperty("value")]
            public decimal Value { get; set; }

            public DateTime GetDepartureDate()
            {
                var formats = new[] { "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-dd", "yyyy-MM" };

                if (DateTime.TryParseExact(
                    DepartDateRaw,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedDate))
                {
                    return parsedDate;
                }

                Console.WriteLine($"Invalid date format: {DepartDateRaw}");
                return DateTime.UtcNow;
            }

            public DateTime GetReturnDepartureDate()
            {
                var formats = new[] { "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-dd", "yyyy-MM" };

                if (DateTime.TryParseExact(
                    ReturnDateRaw,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedDate))
                {
                    return parsedDate;
                }

                Console.WriteLine($"Invalid date format: {ReturnDateRaw}");
                return DateTime.UtcNow;
            }
        }
        private class RapidApiResponse
        {
            [JsonProperty("response")]
            public List<RapidApiAirport> Response { get; set; }
        }

        private class RapidApiAirport
        {
            [JsonProperty("code")] public string IataCode { get; set; }
            [JsonProperty("icao")] public string IcaoCode { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("latitude")] public double Latitude { get; set; }
            [JsonProperty("longitude")] public double Longitude { get; set; }
            [JsonProperty("elevation")] public int Elevation { get; set; }
            [JsonProperty("url")] public string Url { get; set; }
            [JsonProperty("time_zone")] public string TimeZone { get; set; }
            [JsonProperty("city_code")] public string CityCode { get; set; }
            [JsonProperty("country")] public string Country { get; set; }
            [JsonProperty("city")] public string City { get; set; }
            [JsonProperty("state")] public string State { get; set; }
            [JsonProperty("county")] public string County { get; set; }
            [JsonProperty("type")] public string Type { get; set; }
        }
        public record AirportInfo(
            string Name,
            string CityName
        );
    }
}
