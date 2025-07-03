namespace FanRide.Models
{
    public class BookingDetails
    {
        public int BookingId { get; set; }
        public string ConcertName { get; set; }
        public DateTime ConcertDate { get; set; }
        public string ConcertLocation { get; set; }
        public string Landmark { get; set; }
        public DateTime DepartureTime { get; set; }
        public string VehicleType { get; set; }
        public string PlateNumber { get; set; } // optional / "N/A"
        public string SeatType { get; set; }
        public int SeatCount { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string DriverName { get; set; } // ✅ Added
        public string DriverPhone { get; set; } // ✅ Added
    }
}
