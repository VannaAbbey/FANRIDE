using Microsoft.AspNetCore.Mvc;
using FanRide.Models;

namespace FanRide.Controllers
{
    public class EventController : Controller
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
                }
            };
        }

        [HttpPost]
        public IActionResult ExpressInterest(int id)
        {
            // Add logic to increment interest in DB if needed
            TempData["Message"] = "Interest expressed!";
            return RedirectToAction("Dashboard", "Event");
        }
    }
}
