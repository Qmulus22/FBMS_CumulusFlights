using FBMS_CumulusFlights.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace FBMS_CumulusFlights.Services
{
    public class IataDataService
    {
        private readonly IataData _iataData;

        public IataDataService()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "YourProjectName.Data.IataData.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                _iataData = JsonConvert.DeserializeObject<IataData>(json);
            }
        }

        public AirportInfo GetAirportInfo(string iataCode)
        {
            if (_iataData.IATA_MAP.TryGetValue(iataCode.ToUpper(), out var info))
            {
                return info;
            }

            return new AirportInfo
            {
                AirportName = iataCode,
                CityName = iataCode
            };
        }
    }
}
