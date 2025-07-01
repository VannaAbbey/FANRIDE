using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FanRide.Models;
using System.Security.Claims;
using MySql.Data.MySqlClient;

namespace FanRide.Controllers
{
    [Authorize(Roles = "Rider")]
    public class RidersController : Controller
    {
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=Web123;";

        // Dashboard
        public IActionResult Dashboard()
        {
            var events = GetConcertsFromDatabase();
            return View("Dashboard", events);
        }

        // Load concert cards from the database
        private List<Event> GetConcertsFromDatabase()
        {
            var concerts = new List<Event>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var cmd = new MySqlCommand(@"
                        SELECT E.Id, E.Title, E.Artist, E.DateTime, E.Location, E.Description, E.ImageUrl,
                               (SELECT COUNT(*) FROM InterestChecks IC WHERE IC.EventId = E.Id) AS InterestCount
                        FROM Events E", connection);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        concerts.Add(new Event
                        {
                            Id = reader.GetInt32("Id"),
                            Title = reader.GetString("Title"),
                            Artist = reader.GetString("Artist"),
                            Date = reader.GetDateTime("DateTime"),
                            Location = reader.GetString("Location"),
                            Description = reader.GetString("Description"),
                            ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? "/images/default-concert.jpg" : reader.GetString("ImageUrl"),
                            InterestCount = reader.GetInt32("InterestCount")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading events.";
                Console.WriteLine("DB ERROR: " + ex.Message);
            }

            return concerts;
        }

        // Bookings Page for Rider
        public IActionResult MyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = new List<BookingDetails>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var cmd = new MySqlCommand(@"
                        SELECT 
                            B.Id AS BookingId,
                            E.Title AS ConcertName,
                            E.DateTime AS ConcertDate,
                            E.Location AS ConcertLocation,
                            R.DepartureTime,
                            R.CarType AS VehicleType,
                            L.Name AS Landmark,
                            R.PlateNumber,
                            ST.TypeName AS SeatType,
                            B.SeatCount,
                            B.Price,
                            B.Status
                        FROM Bookings B
                        JOIN Rides R ON B.RideId = R.Id
                        JOIN Events E ON R.EventId = E.Id
                        JOIN SeatTypes ST ON B.SeatTypeId = ST.Id
                        JOIN Landmarks L ON R.LandmarkId = L.Id
                        WHERE B.UserId = @UserId;", connection);

                    cmd.Parameters.AddWithValue("@UserId", userId);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        bookings.Add(new BookingDetails
                        {
                            BookingId = reader.GetInt32("BookingId"),
                            ConcertName = reader.GetString("ConcertName"),
                            ConcertDate = reader.GetDateTime("ConcertDate"),
                            ConcertLocation = reader.GetString("ConcertLocation"),
                            Landmark = reader.GetString("Landmark"),
                            DepartureTime = reader.GetDateTime("DepartureTime"),
                            VehicleType = reader.GetString("VehicleType"),
                            PlateNumber = reader.GetString("PlateNumber"),
                            SeatType = reader.GetString("SeatType"),
                            SeatCount = reader.GetInt32("SeatCount"),
                            Price = reader.GetDecimal("Price"),
                            Status = reader.GetString("Status")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Something went wrong while loading your bookings.";
                Console.WriteLine("DB ERROR: " + ex.Message);
            }

            return View("MyBookings", bookings);
        }

        // Express Interest
        [HttpPost]
        public IActionResult ExpressInterest(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    var cmd = new MySqlCommand(@"
                        INSERT INTO InterestChecks (UserId, EventId)
                        VALUES (NULL, @EventId)", connection); // Anonymous interest

                    cmd.Parameters.AddWithValue("@EventId", id);
                    cmd.ExecuteNonQuery();
                }

                TempData["InterestSuccess"] = "✅ Interest recorded!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting interest: " + ex.Message);
            }

            return RedirectToAction("Dashboard", "Riders");
        }
    }
}
