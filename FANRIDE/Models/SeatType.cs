namespace FanRide.Models
{
    public class SeatType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Available { get; set; }
    }
}
