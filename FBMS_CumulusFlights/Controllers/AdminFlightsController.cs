using FBMS_CumulusFlights.Data;
using FBMS_CumulusFlights.Models;
using FBMS_CumulusFlights.Models.Enums;
using FBMS_CumulusFlights.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FBMS_CumulusFlights.Controllers
{
    public class AdminFlightsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const decimal AverageSpeedKmh = 900m; // Average speed for flight duration calculation

        public AdminFlightsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ApproveFlights(FlightSearchViewModel searchModel)
        {
            // Get today's date
            var today = DateTime.UtcNow.Date;

            // Update flights that have already departed
            var pastFlights = await _context.Flights
                .Where(f => f.DepartureDate < today && !f.IsDeleted)
                .ToListAsync();

            foreach (var flight in pastFlights)
            {
                flight.IsDeleted = true; // Mark as deleted
            }

            await _context.SaveChangesAsync(); // Save changes to the database

            // Query for flights that are pending approval and have not been deleted
            var flightsQuery = _context.Flights
                .Where(f => f.ApprovalStatus == ApprovalStatus.Pending && !f.IsDeleted && f.DepartureDate >= today)
                .AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchModel.FlightNumber))
            {
                flightsQuery = flightsQuery.Where(f => f.FlightNumber.Contains(searchModel.FlightNumber));
            }

            if (!string.IsNullOrEmpty(searchModel.From))
            {
                flightsQuery = flightsQuery.Where(f =>
                    f.OriginCity.Contains(searchModel.From) ||
                    f.OriginAirportCode.Contains(searchModel.From) ||
                    f.OriginAirportName.Contains(searchModel.From));
            }

            if (!string.IsNullOrEmpty(searchModel.To))
            {
                flightsQuery = flightsQuery.Where(f =>
                    f.DestinationCity.Contains(searchModel.To) ||
                    f.DestinationAirportCode.Contains(searchModel.To) ||
                    f.DestinationAirportName.Contains(searchModel.To));
            }

            if (searchModel.DepartureDate.HasValue)
            {
                flightsQuery = flightsQuery.Where(f => f.DepartureDate.Date == searchModel.DepartureDate.Value.Date);
            }

            // Apply departure time range filter
            if (searchModel.DepartureTimeStart.HasValue && searchModel.DepartureTimeEnd.HasValue)
            {
                flightsQuery = flightsQuery.Where(f =>
                    f.DepartureDate.TimeOfDay >= searchModel.DepartureTimeStart.Value &&
                    f.DepartureDate.TimeOfDay <= searchModel.DepartureTimeEnd.Value);
            }

            // Apply max price filter
            if (searchModel.MaxPrice.HasValue)
            {
                flightsQuery = flightsQuery.Where(f => f.Price <= searchModel.MaxPrice.Value);
            }

            // Order by DepartureDate ascending
            var flights = await flightsQuery
                .OrderBy(f => f.DepartureDate) // Order by closest departure date
                .ThenBy(f => f.FlightNumber) // Optional: Order by Flight Number if dates are the same
                .ToListAsync();

            var viewModel = new ApproveFlightsViewModel
            {
                Flights = flights,
                SearchModel = searchModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null || flight.IsDeleted)
            {
                return RedirectToAction("ApproveFlights");
            }

            flight.ApprovalStatus = ApprovalStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction("ApproveFlights");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight != null && !flight.IsDeleted)
            {
                flight.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ApproveFlights");
        }

        public async Task<IActionResult> AdminViewFlights(FlightSearchViewModel searchModel)
        {
            var query = _context.Flights
                .Where(f => f.ApprovalStatus == ApprovalStatus.Approved && !f.IsDeleted);

            // Update operational status based on current time
            var now = DateTime.UtcNow;
            var flightsToUpdate = await query.ToListAsync();

            foreach (var flight in flightsToUpdate)
            {
                if (flight.DepartureDate > now && flight.DepartureDate <= now.AddHours(1.5))
                {
                    flight.OperationalStatus = OperationalStatus.Boarding;
                }
                else if (flight.DepartureDate <= now)
                {
                    flight.OperationalStatus = OperationalStatus.Departed;
                }
                else if (flight.DepartureDate.AddHours(flight.FlightDuration) <= now)
                {
                    flight.OperationalStatus = OperationalStatus.Arrived;
                }
                else if (flight.DepartureDate.AddHours(flight.FlightDuration).AddHours(1) <= now)
                {
                    flight.OperationalStatus = OperationalStatus.Completed;
                }
            }

            await _context.SaveChangesAsync(); // Save changes to the database

            // Apply search filters
            if (!string.IsNullOrEmpty(searchModel.FlightNumber))
            {
                query = query.Where(f => f.FlightNumber.Contains(searchModel.FlightNumber));
            }

            if (!string.IsNullOrEmpty(searchModel.From))
            {
                query = query.Where(f => f.OriginCity.Contains(searchModel.From) ||
                                         f.OriginAirportCode.Contains(searchModel.From) ||
                                         f.OriginAirportName.Contains(searchModel.From));
            }

            if (!string.IsNullOrEmpty(searchModel.To))
            {
                query = query.Where(f => f.DestinationCity.Contains(searchModel.To) ||
                                         f.DestinationAirportCode.Contains(searchModel.To) ||
                                         f.DestinationAirportName.Contains(searchModel.To));
            }

            if (searchModel.DepartureDate.HasValue)
            {
                query = query.Where(f => f.DepartureDate.Date == searchModel.DepartureDate.Value.Date);
            }

            if (searchModel.DepartureTimeStart.HasValue && searchModel.DepartureTimeEnd.HasValue)
            {
                query = query.Where(f => f.DepartureDate.TimeOfDay >= searchModel.DepartureTimeStart.Value &&
                                         f.DepartureDate.TimeOfDay <= searchModel.DepartureTimeEnd.Value);
            }

            if (searchModel.MaxPrice.HasValue)
            {
                query = query.Where(f => f.Price <= searchModel.MaxPrice.Value);
            }

            if (searchModel.OperationalStatus.HasValue)
            {
                query = query.Where(f => f.OperationalStatus == searchModel.OperationalStatus.Value);
            }

            var flights = await query
                .OrderBy(f => f.ApprovalStatus) // Order by Approval Status first
                .ThenBy(f => f.DepartureDate) // Then by Departure Date
                .ToListAsync();

            var viewModel = new AdminViewFlightsViewModel
            {
                Flights = flights,
                SearchModel = searchModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOperationalStatus(int id, OperationalStatus status, DateTime? newDepartureDateTime)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight != null)
            {
                flight.OperationalStatus = status;
                if (newDepartureDateTime.HasValue)
                {
                    flight.DepartureDate = newDepartureDateTime.Value;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(AdminViewFlights));
        }

        // GET: AdminFlights/CreateFlight
        public IActionResult CreateFlight()
        {
            return View(new Flight()); // Return a new Flight model to the view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFlight(FlightViewModel flightViewModel)
        {
            if (ModelState.IsValid)
            {
                var flight = new Flight
                {
                    FlightNumber = GenerateFlightNumber(), // Auto-generate Flight Number
                    FlightDuration = CalculateFlightDuration(flightViewModel.DistanceKm), // Calculate Flight Duration
                    AddedAt = DateTime.UtcNow, // Set the added date
                    ApprovalStatus = ApprovalStatus.Pending, // Default status
                    OperationalStatus = OperationalStatus.InProgress, // Default status
                    IsDeleted = false, // Default status
                    SourceType = FlightSource.Manual, // Default SourceType to Manual
                    OriginCity = flightViewModel.OriginCity,
                    OriginAirportCode = flightViewModel.OriginAirportCode,
                    OriginAirportName = flightViewModel.OriginAirportName,
                    DestinationCity = flightViewModel.DestinationCity,
                    DestinationAirportCode = flightViewModel.DestinationAirportCode,
                    DestinationAirportName = flightViewModel.DestinationAirportName,
                    DepartureDate = flightViewModel.DepartureDate,
                    DistanceKm = flightViewModel.DistanceKm,
                    Price = flightViewModel.Price
                };

                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction("AdminViewFlights"); // Redirect to the view flights page
            }

            return View(flightViewModel); // Return the view with the current flight data if validation fails
        }

        private double CalculateFlightDuration(decimal distance)
        {
            return (double)(distance / AverageSpeedKmh); // Calculate duration based on distance and average speed
        }

        private string GenerateFlightNumber()
        {
            return $"FL{DateTime.UtcNow:yyMMddHHmmss}"; // Generate a unique flight number
        }
        public IActionResult AdminAddCreateFlights()
        {
            return View();
        }
    }
}