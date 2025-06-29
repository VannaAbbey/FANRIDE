using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FanRide.Models;

namespace FanRide.Controllers
{
    public class RidersController : Controller
    {
        public IActionResult Dashboard()
        {
            var events = GetSampleConcerts();
            return View("Dashboard", events);
        }

        private List<Event> GetSampleConcerts()
        {
            return new List<Event>
            {
                new Event
                {
                    Id = 1,
                    Title = "Born Pink Tour",
                    Artist = "BLACKPINK",
                    Date = DateTime.Now.AddDays(5),
                    Location = "Philippine Arena",
                    Description = "BLACKPINK returns to Manila with a bang!",
                    ImageUrl = "/images/bornpink.jpg",
                    InterestCount = 230
                },
                new Event
                {
                    Id = 2,
                    Title = "Super One",
                    Artist = "SuperM",
                    Date = DateTime.Now.AddDays(8),
                    Location = "MOA Arena",
                    Description = "Get hyped with SuperM's electrifying show!",
                    ImageUrl = "/images/superm.jpg",
                    InterestCount = 150
                },
                new Event
                 {
                Id = 3,
                Title = "BTS FAN MEET",
                Artist = "BTS",
                Date = new DateTime(2025, 10, 5, 15, 30, 0),
                Location = "MOA Arena",
                Description = "Join BTS for an unforgettable fan meet experience!",
                ImageUrl = "/images/bts.jpg",
                InterestCount = 0
                },
            new Event
            {
                Id = 4,
                Title = "TWICE IN CONCERT",
                Artist = "TWICE",
                Date = new DateTime(2025, 11, 12, 20, 0, 0),
                Location = "Araneta Coliseum",
                Description = "ONCEs get ready! TWICE is coming to town!",
                ImageUrl = "/images/twice.jpg",
                InterestCount = 0
            },

                // Add more sample events if needed
            };
        }
    }
}
