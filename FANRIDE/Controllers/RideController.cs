using Microsoft.AspNetCore.Mvc;
using FanRide.Models; // <-- Import mo dapat ito
using System.Collections.Generic;
using System.Linq;
using System;

namespace FanRide.Controllers
{
    public class RideController : Controller
    {
        private static List<Ride> SampleRides = new List<Ride>
        {
            new Ride
            {
                Id = 1,
                EventId = 1,
                DriverName = "Chris Rider",
                DriverImageUrl = "/images/chris.jpg",
                FromLocation = "SM Megamall, Mandaluyong",
                DepartureTime = DateTime.Today.AddHours(17).AddMinutes(21),
                CarType = "White Honda City",
                SeatTypes = new List<SeatType>
                {
                    new SeatType { TypeId = 101, TypeName = "Front Passenger", Price = 250, Available = 1 },
                    new SeatType { TypeId = 102, TypeName = "Rear Window", Price = 200, Available = 2 }
                }
            },

            new Ride
            {
                Id = 2,
                EventId = 1,
                DriverName = "Jennie Drive",
                DriverImageUrl = "/images/jennie.jpg",
                FromLocation = "Ayala Malls Solenad, Nuvali",
                DepartureTime = DateTime.Today.AddHours(15).AddMinutes(45),
                CarType = "Black Toyota Vios",
                SeatTypes = new List<SeatType>
                {
                    new SeatType { TypeId = 103, TypeName = "Front Passenger", Price = 230, Available = 1 },
                    new SeatType { TypeId = 104, TypeName = "Rear Middle", Price = 180, Available = 1 }
                }
            },
            new Ride
            {
                Id = 3,
                EventId = 1,
                DriverName = "Lisa Wheels",
                DriverImageUrl = "/images/lisa.jpg",
                FromLocation = "Robinsons Tagapo, Sta. Rosa",
                DepartureTime = DateTime.Today.AddHours(16).AddMinutes(10),
                CarType = "Blue Ford EcoSport",
                SeatTypes = new List<SeatType>
                {
                    new SeatType { TypeId = 105, TypeName = "Rear Window", Price = 200, Available = 2 },
                    new SeatType { TypeId = 106, TypeName = "Front Passenger", Price = 240, Available = 1 }
                }
            }
        };

        public IActionResult AvailableRides(int eventId)
        {
            var rides = SampleRides
                .Where(r => r.EventId == eventId)
                .Select(r => new RideViewModel
                {
                    Id = r.Id,
                    DriverName = r.DriverName,
                    DriverImageUrl = r.DriverImageUrl,
                    DepartureLocation = r.FromLocation,
                    DepartureTime = r.DepartureTime,
                    CarDescription = r.CarType,
                    TotalSeatsLeft = r.SeatTypes.Sum(s => s.Available),
                    SeatTypes = r.SeatTypes
                }).ToList();

            return View("AvailableRides", rides);
        }

        [HttpPost]
        public IActionResult BookRide(int rideId, Dictionary<int, int> seatQuantities)
        {
            var ride = SampleRides.FirstOrDefault(r => r.Id == rideId);
            if (ride != null && seatQuantities != null)
            {
                foreach (var seat in ride.SeatTypes)
                {
                    if (seatQuantities.TryGetValue(seat.TypeId, out int qty))
                    {
                        if (qty > 0 && qty <= seat.Available)
                        {
                            seat.Available -= qty;
                        }
                    }
                }
            }

            TempData["Success"] = "Booking successful!";
            return RedirectToAction("AvailableRides", new { eventId = ride?.EventId ?? 1 });
        }
    }

    // REMOVE the class definitions here. Ginawa na natin sa Models folder.
}
