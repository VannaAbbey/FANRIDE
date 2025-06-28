using Microsoft.AspNetCore.Mvc;
using FanRide.Models;
using System.Collections.Generic;
using System.Linq;

namespace FanRide.Controllers
{
    public class RideController : Controller
    {
        // Temporary in-memory ride data (replace with DB access later)
        private static List<Ride> SampleRides = new List<Ride>
        {
            new Ride
            {
                Id = 1,
                EventId = 1,
                DriverName = "Alice",
                SeatsAvailable = 3,
                CarType = "SUV",
                DepartureTime = DateTime.Now.AddHours(2)
            },
            new Ride
            {
                Id = 2,
                EventId = 2,
                DriverName = "Bob",
                SeatsAvailable = 2,
                CarType = "Van",
                DepartureTime = DateTime.Now.AddHours(3)
            }
        };

        public IActionResult AvailableRides(int eventId)
        {
            var filteredRides = SampleRides.Where(r => r.EventId == eventId).ToList();
            ViewBag.EventId = eventId;
            return View("AvailableRides", filteredRides); // Make sure Rides.cshtml exists
        }
    }
}
