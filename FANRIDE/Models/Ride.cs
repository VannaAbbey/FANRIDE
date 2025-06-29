using System;
using System.Collections.Generic;

namespace FanRide.Models
{
    public class Ride
    {
        public int Id { get; set; }

        // Driver info
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverImageUrl { get; set; } = string.Empty;

        // Car info
        public string CarType { get; set; } = string.Empty;
        public string CarDescription { get; set; } = string.Empty;
        public int CarSeatsTotal { get; set; }
        public int SeatsAvailable { get; set; }

        // Seat types (ex: VIP, Regular, Standing)
        public List<SeatType> SeatTypes { get; set; } = new();

        // Ride info
        public string FromLocation { get; set; } = string.Empty;
        public string DepartureLocation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }

        // Event info
        public int EventId { get; set; }

        // Computed property
        public int SeatsTaken => CarSeatsTotal - SeatsAvailable;
    }
}
