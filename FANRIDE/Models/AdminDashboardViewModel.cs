namespace FanRide.Models
{
    public class AdminDashboardViewModel
    {
        public int DriverCount { get; set; }
        public int PassengerCount { get; set; }

        public List<Event> UpcomingEvents { get; set; }

    }
}
