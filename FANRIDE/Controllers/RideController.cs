using Microsoft.AspNetCore.Mvc;
using FanRide.Models;
using System.Collections.Generic;
using System;
using MySql.Data.MySqlClient;
using System.Security.Claims;

namespace FanRide.Controllers
{
    public class RideController : Controller
    {
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=Web123;";
 
        public IActionResult AvailableRides(int eventId)
        {
            var rides = GetAvailableRidesFromDatabase(eventId);
            return View("AvailableRides", rides);
        }

        [HttpPost]
        public IActionResult BookRide(int rideId, Dictionary<int, int> seatQuantities)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (seatQuantities != null)
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    foreach (var kvp in seatQuantities)
                    {
                        int seatTypeId = kvp.Key;
                        int qty = kvp.Value;

                        if (qty > 0)
                        {
                            decimal price = GetSeatPrice(seatTypeId, connection);

                            var command = new MySqlCommand(@"
                                INSERT INTO Bookings (UserId, RideId, SeatTypeId, SeatCount, Price, Status)
                                VALUES (@UserId, @RideId, @SeatTypeId, @SeatCount, @Price, @Status)", connection);

                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@RideId", rideId);
                            command.Parameters.AddWithValue("@SeatTypeId", seatTypeId);
                            command.Parameters.AddWithValue("@SeatCount", qty);
                            command.Parameters.AddWithValue("@Price", price * qty);
                            command.Parameters.AddWithValue("@Status", "Confirmed");

                            command.ExecuteNonQuery();
                        }
                    }
                }

                TempData["Success"] = "Booking successful!";
                return RedirectToAction("AvailableRides", new { eventId = 1 });
            }

            TempData["Error"] = "Booking failed. Please try again.";
            return RedirectToAction("AvailableRides", new { eventId = 1 });
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            return 0;
        }

        private decimal GetSeatPrice(int seatTypeId, MySqlConnection connection)
        {
            var cmd = new MySqlCommand("SELECT CASE WHEN Id = 1 THEN 250 WHEN Id = 2 THEN 200 ELSE 0 END FROM SeatTypes WHERE Id = @Id", connection);
            cmd.Parameters.AddWithValue("@Id", seatTypeId);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToDecimal(result) : 0;
        }

        private List<RideViewModel> GetAvailableRidesFromDatabase(int eventId)
        {
            var rides = new List<RideViewModel>();
            var filteredRides = new List<RideViewModel>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var rideCmd = new MySqlCommand(@"
                    SELECT R.Id AS RideId, R.DriverId, R.CarType, R.CarSeatsTotal, R.DepartureTime,
                           U.FirstName, U.LastName
                    FROM Rides R
                    JOIN Users U ON R.DriverId = U.Id
                    WHERE R.EventId = @EventId AND R.IsApproved = TRUE", connection);

                rideCmd.Parameters.AddWithValue("@EventId", eventId);
                var reader = rideCmd.ExecuteReader();

                while (reader.Read())
                {
                    rides.Add(new RideViewModel
                    {
                        Id = reader.GetInt32("RideId"),
                        DriverName = reader.GetString("FirstName") + " " + reader.GetString("LastName"),
                        DepartureLocation = "Pickup Point",
                        DepartureTime = reader.GetDateTime("DepartureTime"),
                        CarDescription = reader.GetString("CarType"),
                        DriverImageUrl = "/images/default-driver.jpg",
                        TotalSeatsLeft = 0,
                        SeatTypes = new List<SeatType>()
                    });
                }
                reader.Close();

                foreach (var ride in rides)
                {
                    var seatCmd = new MySqlCommand(@"
                        SELECT 
                            S.Id AS TypeId,
                            S.TypeName,
                            S.TotalSeats - IFNULL(SUM(B.SeatCount), 0) AS AvailableSeats
                        FROM SeatTypes S
                        LEFT JOIN Bookings B ON B.SeatTypeId = S.Id AND B.RideId = @RideId
                        GROUP BY S.Id, S.TypeName, S.TotalSeats", connection);

                    seatCmd.Parameters.AddWithValue("@RideId", ride.Id);
                    var seatReader = seatCmd.ExecuteReader();

                    var seatTypes = new List<SeatType>();
                    int totalAvailable = 0;

                    while (seatReader.Read())
                    {
                        int available = seatReader.GetInt32("AvailableSeats");
                        totalAvailable += available;

                        seatTypes.Add(new SeatType
                        {
                            TypeId = seatReader.GetInt32("TypeId"),
                            TypeName = seatReader.GetString("TypeName"),
                            Price = seatReader.GetInt32("TypeId") == 1 ? 250 : 200,
                            Available = available
                        });
                    }

                    seatReader.Close();

                    // Only include rides with available seats
                    if (totalAvailable > 0)
                    {
                        ride.SeatTypes = seatTypes;
                        ride.TotalSeatsLeft = totalAvailable;
                        filteredRides.Add(ride);
                    }
                }

                connection.Close();
            }

            return filteredRides;
        }
    }
}
