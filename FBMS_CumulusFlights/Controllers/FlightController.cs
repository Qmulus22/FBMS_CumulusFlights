using Microsoft.AspNetCore.Mvc;

namespace Cumulus_Flights.Controllers
{
    public class FlightController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
