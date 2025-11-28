using System.Text.Json.Serialization;
using FBMS_CumulusFlights.Models.Enums;

namespace FBMS_CumulusFlights.ViewModels
{
    public class PassengerViewModel
    {
        public int Index { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PassportNumber { get; set; }
        public string PassportCountry { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public decimal BaggageWeight { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BaggageTypeEnum BaggageType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SeatClassEnum SeatClass { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SeatTypeEnum SeatType { get; set; }

        public decimal BaggageFee { get; set; }
        public decimal SeatFee { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PassengerType PassengerClassification { get; set; }
    }
}