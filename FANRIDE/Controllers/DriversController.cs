using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FanRide.Models;
using MySql.Data.MySqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FanRide.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriversController : Controller
    {
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=1234;";

        public IActionResult Dashboard()
        {
            var events = new List<Event>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                var cmd = new MySqlCommand(@"
            SELECT Id, Title, Artist, DateTime, Location, Description, ImageUrl
            FROM Events
            ORDER BY DateTime ASC;", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var imageFile = reader.IsDBNull(reader.GetOrdinal("ImageUrl"))
                        ? "default-concert.jpg"
                        : reader.GetString("ImageUrl");

                    events.Add(new Event
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        Artist = reader.GetString("Artist"),
                        Date = reader.GetDateTime("DateTime"),
                        Location = reader.GetString("Location"),
                        Description = reader.GetString("Description"),
                        ImageUrl = $"/images/{imageFile}"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Driver Dashboard Load Error: " + ex.Message);
                ViewBag.Error = "Failed to load events.";
            }

            return View("Dashboard", events);
        }

        public IActionResult AddRide(int eventId)
        {
            var model = new RideViewModel
            {
                EventId = eventId,
                EventTitle = GetEventTitleById(eventId),
                DepartureTime = DateTime.Now,
                LandmarkSelectList = GetLandmarks().Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                }).ToList()
            };

            if (TempData["Success"] != null)
                ViewBag.Success = TempData["Success"];

            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View("AddRide", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddRide(RideViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int driverId))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.LandmarkSelectList = GetLandmarks().Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                }).ToList();
                model.EventTitle = GetEventTitleById(model.EventId);
                return View("AddRide", model);
            }

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                int carSeatsTotal = 1 + model.RearSeatCount;

                var cmd = new MySqlCommand(@"
                    INSERT INTO Rides 
                    (DriverId, EventId, LandmarkId, CarType, PlateNumber, CarSeatsTotal, DepartureTime, IsApproved)
                    VALUES 
                    (@DriverId, @EventId, @LandmarkId, @CarType, @PlateNumber, @CarSeatsTotal, @DepartureTime, TRUE);", conn);

                cmd.Parameters.AddWithValue("@DriverId", driverId);
                cmd.Parameters.AddWithValue("@EventId", model.EventId);
                cmd.Parameters.AddWithValue("@LandmarkId", model.LandmarkId);
                cmd.Parameters.AddWithValue("@CarType", model.CarType);
                cmd.Parameters.AddWithValue("@PlateNumber", model.PlateNumber);
                cmd.Parameters.AddWithValue("@CarSeatsTotal", carSeatsTotal);
                cmd.Parameters.AddWithValue("@DepartureTime", model.DepartureTime);

                cmd.ExecuteNonQuery();
                long newRideId = cmd.LastInsertedId;

                var frontCmd = new MySqlCommand("INSERT INTO RideSeats (RideId, SeatTypeId, TotalSeats) VALUES (@RideId, 1, 1);", conn);
                frontCmd.Parameters.AddWithValue("@RideId", newRideId);
                frontCmd.ExecuteNonQuery();

                if (model.RearSeatCount > 0)
                {
                    var rearCmd = new MySqlCommand("INSERT INTO RideSeats (RideId, SeatTypeId, TotalSeats) VALUES (@RideId, 2, @TotalSeats);", conn);
                    rearCmd.Parameters.AddWithValue("@RideId", newRideId);
                    rearCmd.Parameters.AddWithValue("@TotalSeats", model.RearSeatCount);
                    rearCmd.ExecuteNonQuery();
                }

                TempData["Success"] = "✅ Ride posted successfully!";
                return RedirectToAction("AddRide", new { eventId = model.EventId });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Add Ride Error: " + ex.Message);
                TempData["Error"] = "❌ Something went wrong while posting the ride.";
                model.LandmarkSelectList = GetLandmarks().Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                }).ToList();
                model.EventTitle = GetEventTitleById(model.EventId);
                return View("AddRide", model);
            }
        }

        public IActionResult MyRides()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int driverId))
                return RedirectToAction("Login", "Account");

            var rideList = new List<RideWithPassengersViewModel>();

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                var cmd = new MySqlCommand(@"
                    SELECT R.Id AS RideId, E.Title AS ConcertTitle, E.Location, L.Name AS LandmarkName,
                           R.DepartureTime, R.CarType, R.PlateNumber, R.CarSeatsTotal
                    FROM Rides R
                    JOIN Events E ON R.EventId = E.Id
                    JOIN Landmarks L ON R.LandmarkId = L.Id
                    WHERE R.DriverId = @DriverId
                    ORDER BY R.DepartureTime DESC;", connection);

                cmd.Parameters.AddWithValue("@DriverId", driverId);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    rideList.Add(new RideWithPassengersViewModel
                    {
                        RideId = reader.GetInt32("RideId"),
                        ConcertTitle = reader.GetString("ConcertTitle"),
                        Location = reader.GetString("Location"),
                        LandmarkName = reader.GetString("LandmarkName"),
                        DepartureTime = reader.GetDateTime("DepartureTime"),
                        CarType = reader.GetString("CarType"),
                        PlateNumber = reader.GetString("PlateNumber"),
                        CarSeatsTotal = reader.GetInt32("CarSeatsTotal")
                    });
                }

                reader.Close();

                foreach (var ride in rideList)
                {
                    var seatCmd = new MySqlCommand(@"
                        SELECT TotalSeats FROM RideSeats 
                        WHERE RideId = @RideId AND SeatTypeId = 2;", connection);

                    seatCmd.Parameters.AddWithValue("@RideId", ride.RideId);
                    var result = seatCmd.ExecuteScalar();
                    ride.RearSeats = result != null ? Convert.ToInt32(result) : 0;

                    var pCmd = new MySqlCommand(@"
                        SELECT U.FirstName, U.LastName, U.PhoneNumber, B.SeatCount, ST.TypeName
                        FROM Bookings B
                        JOIN Users U ON B.UserId = U.Id
                        JOIN SeatTypes ST ON B.SeatTypeId = ST.Id
                        WHERE B.RideId = @RideId;", connection);

                    pCmd.Parameters.AddWithValue("@RideId", ride.RideId);
                    var pReader = pCmd.ExecuteReader();

                    while (pReader.Read())
                    {
                        ride.Passengers.Add(new PassengerInfo
                        {
                            Name = $"{pReader.GetString("FirstName")} {pReader.GetString("LastName")}",
                            PhoneNumber = pReader.GetString("PhoneNumber"),
                            SeatCount = pReader.GetInt32("SeatCount"),
                            SeatType = pReader.GetString("TypeName")
                        });
                    }
                    pReader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MyRides ERROR: " + ex.Message);
                ViewBag.Error = "Failed to load rides.";
            }

            return View("MyRides", rideList);
        }

        private List<Landmark> GetLandmarks()
        {
            var landmarks = new List<Landmark>();
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = new MySqlCommand("SELECT * FROM Landmarks", connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                landmarks.Add(new Landmark
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Province = reader.GetString("Province")
                });
            }

            return landmarks;
        }

        private string GetEventTitleById(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("SELECT Title FROM Events WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "(Unknown Event)";
        }

        private List<SeatType> GetSeatTypes()
        {
            var seatTypes = new List<SeatType>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            var cmd = new MySqlCommand("SELECT Id, TypeName, Price FROM SeatTypes", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                seatTypes.Add(new SeatType
                {
                    TypeId = reader.GetInt32("Id"),
                    TypeName = reader.GetString("TypeName"),
                    Price = reader.GetDecimal("Price")
                });
            }
            return seatTypes;
        }
    }
}
