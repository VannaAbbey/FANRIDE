using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FanRide.Models;
using MySql.Data.MySqlClient;
using System.IO;

namespace FanRide.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=1234;";

        // Admin Dashboard showing stats
        public IActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT Role, COUNT(*) AS Count FROM Users GROUP BY Role", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var role = reader.GetString("Role");
                var count = reader.GetInt32("Count");

                if (role == "Driver") model.DriverCount = count;
                else if (role == "Rider") model.PassengerCount = count;
            }

            return View("Dashboard", model);
        }

        // GET: /Admin/AddEvent
        public IActionResult AddEvent()
        {
            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddEvent(Event model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            string fileName = "default-concert.jpg"; // fallback

            // ✅ Save uploaded file to wwwroot/images
            if (ImageFile != null && ImageFile.Length > 0)
            {
                try
                {
                    string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    fileName = Path.GetFileName(ImageFile.FileName); // e.g., bts.jpg
                    string filePath = Path.Combine(uploadsPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    ImageFile.CopyTo(stream);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "❌ Failed to upload image. " + ex.Message;
                    return View(model);
                }
            }

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                var cmd = new MySqlCommand(@"
                    INSERT INTO Events (Title, Artist, DateTime, Location, Description, ImageUrl)
                    VALUES (@Title, @Artist, @DateTime, @Location, @Description, @ImageUrl);", conn);

                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters.AddWithValue("@Artist", model.Artist);
                cmd.Parameters.AddWithValue("@DateTime", model.Date);
                cmd.Parameters.AddWithValue("@Location", model.Location);
                cmd.Parameters.AddWithValue("@Description", model.Description);
                cmd.Parameters.AddWithValue("@ImageUrl", fileName); // ✅ only save 'bts.jpg'

                cmd.ExecuteNonQuery();

                TempData["Success"] = "✅ Event added successfully!";
                return RedirectToAction("AddEvent");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Failed to add event. " + ex.Message;
                return View(model);
            }
        }

        // ✅ NEW: View All Users
        public IActionResult AdminPanel()
        {
            var model = new AdminPanelViewModel
            {
                Users = new List<User>(),
                Events = new List<Event>(),
                Rides = new List<Ride>(),
                Bookings = new List<BookingDetails>()
            };

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            // Users
            var cmdUsers = new MySqlCommand("SELECT * FROM Users", conn);
            using var readerUsers = cmdUsers.ExecuteReader();
            while (readerUsers.Read())
            {
                model.Users.Add(new User
                {
                    Id = readerUsers.GetInt32("Id"),
                    FirstName = readerUsers.GetString("FirstName"),
                    MiddleName = readerUsers.GetString("MiddleName"),
                    LastName = readerUsers.GetString("LastName"),
                    Email = readerUsers.GetString("Email"),
                    Province = readerUsers.GetString("Province"),
                    PhoneNumber = readerUsers.GetString("PhoneNumber"),
                    Role = readerUsers.GetString("Role"),
                    CreatedAt = readerUsers.GetDateTime("CreatedAt")
                });
            }

            readerUsers.Close(); // Important before next reader

            // Events
            var cmdEvents = new MySqlCommand("SELECT * FROM Events", conn);
            using var readerEvents = cmdEvents.ExecuteReader();
            while (readerEvents.Read())
            {
                model.Events.Add(new Event
                {
                    Id = readerEvents.GetInt32("Id"),
                    Title = readerEvents.GetString("Title"),
                    Artist = readerEvents.GetString("Artist"),
                    Date = readerEvents.GetDateTime("DateTime"),
                    Location = readerEvents.GetString("Location"),
                    Description = readerEvents.GetString("Description"),
                    ImageUrl = readerEvents.GetString("ImageUrl")
                });
            }

            readerEvents.Close(); // Important before next reader

            // Rides
            var cmdRides = new MySqlCommand("SELECT * FROM Rides", conn);
            using var readerRides = cmdRides.ExecuteReader();
            while (readerRides.Read())
            {
                model.Rides.Add(new Ride
                {
                    Id = readerRides.GetInt32("Id"),
                    DriverId = readerRides.GetInt32("DriverId"),
                    EventId = readerRides.GetInt32("EventId"),
                    CarType = readerRides.GetString("CarType"),
                    CarSeatsTotal = readerRides.GetInt32("CarSeatsTotal"),
                    DepartureTime = readerRides.GetDateTime("DepartureTime")
                });
            }

            readerRides.Close();

            // Bookings
            var cmdBookings = new MySqlCommand(@"
            SELECT 
                B.Id as BookingId,
                B.SeatCount,
                B.Price,
                B.Status,
                B.BookingDate,
                E.Title AS ConcertName,
                E.DateTime AS ConcertDate,
                E.Location AS ConcertLocation,
                R.DepartureTime,
                R.CarType,
                R.PlateNumber,
                L.Name AS Landmark,
                S.TypeName AS SeatType
            FROM Bookings B
            INNER JOIN Rides R ON B.RideId = R.Id
            INNER JOIN Events E ON R.EventId = E.Id
            INNER JOIN Landmarks L ON R.LandmarkId = L.Id
            INNER JOIN SeatTypes S ON B.SeatTypeId = S.Id", conn);
            using var readerBookings = cmdBookings.ExecuteReader();
            while (readerBookings.Read())
            {
                model.Bookings.Add(new BookingDetails
                {
                    BookingId = readerBookings.GetInt32("BookingId"),
                    ConcertName = readerBookings.GetString("ConcertName"),
                    ConcertDate = readerBookings.GetDateTime("ConcertDate"),
                    ConcertLocation = readerBookings.GetString("ConcertLocation"),
                    Landmark = readerBookings.GetString("Landmark"),
                    DepartureTime = readerBookings.GetDateTime("DepartureTime"),
                    VehicleType = readerBookings.GetString("CarType"),
                    SeatType = readerBookings.GetString("SeatType"),
                    SeatCount = readerBookings.GetInt32("SeatCount"),
                    Price = readerBookings.GetDecimal("Price"),
                    Status = readerBookings.GetString("Status"),
                });
            }

            return View("AdminPanel", model);
        }

    }
}
