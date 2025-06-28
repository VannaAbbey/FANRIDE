using FanRide.Models;
using Microsoft.AspNetCore.Mvc;

namespace FanRide.Controllers
{
    public class EventController : Controller
    {
        private static List<Event> Events = new List<Event>
    {
        new Event
            {
                Id = 1,
                Title = "BLACKPINK WORLD TOUR",
                Artist = "BLACKPINK",
                Location = "Philippine Arena",
                Description = "BLACKPINK returns for their world tour in Manila!",
                ImageUrl = "/images/blackpink.jpg",
                InterestCount = 0
            },
            new Event
            {
                Id = 2,
                Title = "BTS FAN MEET",
                Artist = "BTS",
                Location = "MOA Arena",
                Description = "Join BTS for an unforgettable fan meet experience!",
                ImageUrl = "/images/bts.jpg",
                InterestCount = 0
            },
            new Event
            {
                Id = 3,
                Title = "TWICE IN CONCERT",
                Artist = "TWICE",
                Location = "Araneta Coliseum",
                Description = "ONCEs get ready! TWICE is coming to town!",
                ImageUrl = "/images/twice.jpg",
                InterestCount = 0
            }
    };

        public IActionResult Dashboard()
        {
            return View("Dashboard", Events);
        }

        [HttpPost]
        public IActionResult ExpressInterest(int id)
        {
            var ev = Events.FirstOrDefault(e => e.Id == id);
            if (ev != null)
            {
                ev.InterestCount++;
            }

            return RedirectToAction("Dashboard");
        }
    }
}
