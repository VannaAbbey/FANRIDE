namespace FanRide.Models
{
    public class RideInputModel
    {
        public int EventId { get; set; }
        public int LandmarkId { get; set; }
        public string CarType { get; set; }
        public string PlateNumber { get; set; }
        public int CarSeatsTotal { get; set; }
        public DateTime DepartureTime { get; set; }
    }
}
