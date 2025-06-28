namespace FanRide.Models
{
    public class Ride
    {
        public int Id { get; set; }
        public string DriverName { get; set; } = string.Empty;// Add this if not present
        public string CarType { get; set; } = string.Empty;
        public int CarSeatsTotal { get; set; }  // ✅ REQUIRED
        public int SeatsAvailable { get; set; }     // ✅ For available seat logic
        public DateTime DepartureTime { get; set; }

        // Optional: Link to Event
        public int EventId { get; set; }

        // ✅ Automatically computed property
        public int SeatsTaken => CarSeatsTotal - SeatsAvailable;
    }
}
