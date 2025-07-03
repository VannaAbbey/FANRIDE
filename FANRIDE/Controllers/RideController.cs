using Microsoft.AspNetCore.Mvc;
using FanRide.Models;
using System.Collections.Generic;
using System;
using MySql.Data.MySqlClient;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace FanRide.Controllers
{
    public class RideController : Controller
    {
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=1234;";

        public IActionResult AvailableRides(int eventId)
        {
            var rides = GetAvailableRidesFromDatabase(eventId);
            return View("AvailableRides", rides);
        }

        [HttpPost]
        public IActionResult BookRide(int rideId, Dictionary<int, int> seatQuantities)
        {
            int userId = GetCurrentUserId();

            Console.WriteLine("🛑 Booking initiated by UserId: " + userId);

            if (userId == 0)
            {
                Console.WriteLine("❌ No valid user ID found in claims.");
                return RedirectToAction("Login", "Account");
            }

            int eventId = 0;
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using (var eventCmd = new MySqlCommand("SELECT EventId FROM Rides WHERE Id = @RideId", connection))
            {
                eventCmd.Parameters.AddWithValue("@RideId", rideId);
                var result = eventCmd.ExecuteScalar();
                if (result != null)
                    eventId = Convert.ToInt32(result);
            }

            if (seatQuantities != null)
            {
                foreach (var kvp in seatQuantities)
                {
                    int seatTypeId = kvp.Key;
                    int qty = kvp.Value;

                    if (qty > 0)
                    {
                        decimal pricePerSeat = GetSeatPriceFromDb(seatTypeId, connection);
                        decimal totalPrice = pricePerSeat * qty;

                        Console.WriteLine($"✅ Booking RideId: {rideId}, SeatTypeId: {seatTypeId}, Qty: {qty}, Price: {totalPrice}");

                        var command = new MySqlCommand(@"
                            INSERT INTO Bookings (UserId, RideId, SeatTypeId, SeatCount, Price, Status)
                            VALUES (@UserId, @RideId, @SeatTypeId, @SeatCount, @Price, @Status)", connection);

                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@RideId", rideId);
                        command.Parameters.AddWithValue("@SeatTypeId", seatTypeId);
                        command.Parameters.AddWithValue("@SeatCount", qty);
                        command.Parameters.AddWithValue("@Price", totalPrice);
                        command.Parameters.AddWithValue("@Status", "Confirmed");

                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "✅ Booking successful!";
                return RedirectToAction("AvailableRides", new { eventId });
            }

            TempData["Error"] = "⚠️ Booking failed. Please try again.";
            return RedirectToAction("AvailableRides", new { eventId });
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("🔍 GetCurrentUserId: " + userIdStr);
            return int.TryParse(userIdStr, out int userId) ? userId : 0;
        }

        private decimal GetSeatPriceFromDb(int seatTypeId, MySqlConnection connection)
        {
            var cmd = new MySqlCommand("SELECT Price FROM SeatTypes WHERE Id = @Id", connection);
            cmd.Parameters.AddWithValue("@Id", seatTypeId);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToDecimal(result) : 0;
        }

        private List<RideViewModel> GetAvailableRidesFromDatabase(int eventId)
        {
            var rides = new List<RideViewModel>();
            var filteredRides = new List<RideViewModel>();

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var rideCmd = new MySqlCommand(@"
                SELECT R.Id AS RideId, R.DriverId, R.CarType, R.CarSeatsTotal, R.DepartureTime,
                       U.FirstName, U.LastName, L.Name AS LandmarkName
                FROM Rides R
                JOIN Users U ON R.DriverId = U.Id
                JOIN Landmarks L ON R.LandmarkId = L.Id
                WHERE R.EventId = @EventId AND R.IsApproved = TRUE", connection);

            rideCmd.Parameters.AddWithValue("@EventId", eventId);
            using var reader = rideCmd.ExecuteReader();
            while (reader.Read())
            {
                rides.Add(new RideViewModel
                {
                    RideId = reader.GetInt32("RideId"),
                    DriverName = $"{reader.GetString("FirstName")} {reader.GetString("LastName")}",
                    DepartureLocation = reader.GetString("LandmarkName"),
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
                        RS.SeatTypeId AS TypeId,
                        ST.TypeName,
                        ST.Price,
                        RS.TotalSeats - IFNULL(SUM(B.SeatCount), 0) AS AvailableSeats
                    FROM RideSeats RS
                    JOIN SeatTypes ST ON RS.SeatTypeId = ST.Id
                    LEFT JOIN Bookings B ON B.SeatTypeId = RS.SeatTypeId AND B.RideId = RS.RideId
                    WHERE RS.RideId = @RideId
                    GROUP BY RS.SeatTypeId, ST.TypeName, ST.Price, RS.TotalSeats", connection);

                seatCmd.Parameters.AddWithValue("@RideId", ride.RideId);
                using var seatReader = seatCmd.ExecuteReader();

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
                        Price = seatReader.GetDecimal("Price"),
                        Available = available
                    });
                }

                seatReader.Close();

                if (totalAvailable > 0)
                {
                    ride.SeatTypes = seatTypes;
                    ride.TotalSeatsLeft = totalAvailable;
                    filteredRides.Add(ride);
                }
            }

            return filteredRides;
        }
    }
}
