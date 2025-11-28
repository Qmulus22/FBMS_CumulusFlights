// Models/ViewModels/UserDetailsViewModel.cs
namespace FBMS_CumulusFlights.Models.ViewModels
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FormattedDateOfBirth { get; set; } // Formatted string
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string Address { get; set; }
        public string UserType { get; set; }
        public string Status { get; set; }
        public string DateJoined { get; set; }
    }
}