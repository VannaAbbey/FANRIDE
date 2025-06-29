namespace FanRide.Models
{
    public class RideViewModel
    {
        public int Id { get; set; }
        public string DriverName { get; set; }
        public string DriverImageUrl { get; set; } = "/images/default-driver.jpg"; // default fallback
        public string DepartureLocation { get; set; }
        public DateTime DepartureTime { get; set; }
        public string CarDescription { get; set; }
        public int TotalSeatsLeft { get; set; }

        public List<SeatType> SeatTypes { get; set; } = new List<SeatType>();
    }

    
}

