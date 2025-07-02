using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FanRide.Models;
using System.Security.Claims;
using MySql.Data.MySqlClient;
using System.Data;

namespace FanRide.Controllers
{
    [Authorize(Roles = "Rider")]
    public class RidersController : Controller
    {
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=Web123;";

        // Rider Dashboard
        public IActionResult Dashboard()
        {
            var events = GetConcertsFromDatabase();
            return View("Dashboard", events);
        }

        // Load concerts
        private List<Event> GetConcertsFromDatabase()
        {
            var concerts = new List<Event>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
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
            catch (Exception ex)
            {
                Console.WriteLine("DB ERROR (Dashboard): " + ex.Message);
                ViewBag.Error = "Could not load concerts.";
            }

            return concerts;
        }

        public IActionResult MyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine("🔎 MyBookings() called for user ID: " + userId);

            var bookings = new List<BookingDetails>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var cmd = new MySqlCommand(@"
            SELECT 
                B.Id AS BookingId,
                E.Title AS ConcertName,
                E.DateTime AS ConcertDate,
                E.Location AS ConcertLocation,
                R.DepartureTime,
                R.CarType AS VehicleType,
                R.PlateNumber,
                L.Name AS Landmark,
                ST.TypeName AS SeatType,
                B.SeatCount,
                B.Price,
                B.Status,
                U.FirstName AS DriverFirstName,
                U.LastName AS DriverLastName,
                U.PhoneNumber AS DriverPhone
            FROM Bookings B
            INNER JOIN Rides R ON B.RideId = R.Id
            INNER JOIN Events E ON R.EventId = E.Id
            INNER JOIN Landmarks L ON R.LandmarkId = L.Id
            INNER JOIN SeatTypes ST ON B.SeatTypeId = ST.Id
            INNER JOIN Users U ON R.DriverId = U.Id
            WHERE B.UserId = @UserId", connection);

                cmd.Parameters.AddWithValue("@UserId", userId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // 🔍 DEBUG: Log which fields are null
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.WriteLine($"{reader.GetName(i)} is null? {reader.IsDBNull(i)}");
                    }

                    bookings.Add(new BookingDetails
                    {
                        BookingId = reader.GetInt32("BookingId"),
                        ConcertName = reader.IsDBNull(reader.GetOrdinal("ConcertName")) ? "N/A" : reader.GetString("ConcertName"),
                        ConcertDate = reader.IsDBNull(reader.GetOrdinal("ConcertDate")) ? DateTime.MinValue : reader.GetDateTime("ConcertDate"),
                        ConcertLocation = reader.IsDBNull(reader.GetOrdinal("ConcertLocation")) ? "N/A" : reader.GetString("ConcertLocation"),
                        DepartureTime = reader.IsDBNull(reader.GetOrdinal("DepartureTime")) ? DateTime.MinValue : reader.GetDateTime("DepartureTime"),
                        VehicleType = reader.IsDBNull(reader.GetOrdinal("VehicleType")) ? "Unknown" : reader.GetString("VehicleType"),
                        PlateNumber = reader.IsDBNull(reader.GetOrdinal("PlateNumber")) ? "N/A" : reader.GetString("PlateNumber"),
                        Landmark = reader.IsDBNull(reader.GetOrdinal("Landmark")) ? "N/A" : reader.GetString("Landmark"),
                        SeatType = reader.IsDBNull(reader.GetOrdinal("SeatType")) ? "N/A" : reader.GetString("SeatType"),
                        SeatCount = reader.IsDBNull(reader.GetOrdinal("SeatCount")) ? 0 : reader.GetInt32("SeatCount"),
                        Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDecimal("Price"),
                        Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "N/A" : reader.GetString("Status"),
                        DriverName =
                            (reader.IsDBNull(reader.GetOrdinal("DriverFirstName")) ? "" : reader.GetString("DriverFirstName")) + " " +
                            (reader.IsDBNull(reader.GetOrdinal("DriverLastName")) ? "" : reader.GetString("DriverLastName")),
                        DriverPhone = reader.IsDBNull(reader.GetOrdinal("DriverPhone")) ? "N/A" : reader.GetString("DriverPhone")
                    });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Something went wrong while loading your bookings.";
                Console.WriteLine("❌ DB ERROR (MyBookings): " + ex.Message);
            }

            return View("MyBookings", bookings);
        }

        // Express interest anonymously
        [HttpPost]
        public IActionResult ExpressInterest(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var cmd = new MySqlCommand(@"
                    INSERT INTO InterestChecks (UserId, EventId)
                    VALUES (NULL, @EventId)", connection);

                cmd.Parameters.AddWithValue("@EventId", id);
                cmd.ExecuteNonQuery();

                TempData["InterestSuccess"] = "✅ Interest recorded!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB ERROR (ExpressInterest): " + ex.Message);
            }

            return RedirectToAction("Dashboard");
        }
    }
}
