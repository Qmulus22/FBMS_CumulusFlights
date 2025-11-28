using System;
using System.Collections.Generic;

namespace FBMS_CumulusFlights.ViewModel
{
    public class UserDashboardViewModel
    {
        public string FirstName { get; set; }
        public List<ReservationViewModel> Reservations { get; set; }
        public ReservationViewModel NextTrip { get; set; }

    }
}
