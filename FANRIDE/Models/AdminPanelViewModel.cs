using System.Collections.Generic;

namespace FanRide.Models
{
    public class AdminPanelViewModel
    {
        public List<User> Users { get; set; }
        public List<Event> Events { get; set; }
        public List<Ride> Rides { get; set; }
        public List<BookingDetails> Bookings { get; set; }
    }
}
