using static FBMS_CumulusFlights.Services.FlightDataService;

namespace FBMS_CumulusFlights.Models
{
    public class IataData
    {
        public Dictionary<string, AirportInfo> IATA_MAP { get; set; }
    }

    public class AirportInfo
    {
        public string AirportName { get; set; }
        public string CityName { get; set; }
    }
}
