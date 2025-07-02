using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FanRide.Models
{
    public class RideViewModel
    {
        // Used for editing/deleting rides
        public int RideId { get; set; }

        // Display purposes (Driver's info)
        public string DriverName { get; set; } = "";
        public string DriverImageUrl { get; set; } = "/images/default-driver.jpg";
        public string DepartureLocation { get; set; } = "";
        public string CarDescription { get; set; } = "";
        public int TotalSeatsLeft { get; set; }

        // ------------ Form Fields (input) ------------

        [Required]
        public int EventId { get; set; }

        public string EventTitle { get; set; } = ""; // Display only, not input

        [Required(ErrorMessage = "Pickup landmark is required.")]
        public int LandmarkId { get; set; }

        [Required(ErrorMessage = "Plate number is required.")]
        public string PlateNumber { get; set; } = "";

        [Range(0, 20, ErrorMessage = "Rear seat count must be between 0 and 20.")]
        public int RearSeatCount { get; set; } = 0;

        // ✅ Not required from form — auto-calculated as: 1 + RearSeatCount
        public int CarSeatsTotal { get; set; }

        [Required(ErrorMessage = "Please select a departure time.")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Please describe your car.")]
        public string CarType { get; set; } = "";

        // ------------ Dropdown Select Lists ------------

        public IEnumerable<SelectListItem>? LandmarkSelectList { get; set; }
        public IEnumerable<SelectListItem>? SeatTypeSelectList { get; set; } // Optional
        public IEnumerable<SelectListItem>? EventSelectList { get; set; } // For Admin/Event Selection

        // ------------ Optional Model Lists ------------

        public List<Event>? Events { get; set; }
        public List<Landmark>? Landmarks { get; set; }
        public List<SeatType>? SeatTypes { get; set; }
    }

    public class Landmark
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Province { get; set; } = "";
    }

    public class SeatType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = "";
        public int TotalSeats { get; set; }
        public int Available { get; set; }
        public decimal Price { get; set; }
    }
}
