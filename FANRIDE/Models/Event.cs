namespace FanRide.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; } // example: "bts.jpg"


        // ✅ Add this if not present
        public int InterestCount { get; set; }
    }
}
