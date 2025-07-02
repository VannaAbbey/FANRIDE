using System;
using System.Collections.Generic;

namespace FanRide.Models
{
    public class RideWithPassengersViewModel
    {
        public int RideId { get; set; }
        public string ConcertTitle { get; set; }
        public string Location { get; set; }
        public string LandmarkName { get; set; }
        public DateTime DepartureTime { get; set; }
        public string CarType { get; set; }
        public string PlateNumber { get; set; }
        public int CarSeatsTotal { get; set; }

        public int RearSeats { get; set; }
        public int TotalSeats { get; set; }
        

        public List<PassengerInfo> Passengers { get; set; } = new();
    }

    public class PassengerInfo
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        public string SeatType { get; set; } = string.Empty; // e.g. "VIP" or "Regular"
        public int SeatCount { get; set; }
    }
}
