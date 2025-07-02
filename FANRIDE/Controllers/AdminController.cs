using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FanRide.Models;
using MySql.Data.MySqlClient;

namespace FanRide.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=Web123;";

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            int driverCount = 0;
            int riderCount = 0;

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT Role, COUNT(*) AS Count FROM Users GROUP BY Role", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var role = reader.GetString("Role");
                var count = reader.GetInt32("Count");

                if (role == "Driver") driverCount = count;
                else if (role == "Rider") riderCount = count;
            }

            ViewBag.DriverCount = driverCount;
            ViewBag.RiderCount = riderCount;

            return View("Dashboard");
        }

        // GET: /Admin/AddEvent
        public IActionResult AddEvent()
        {
            return View();
        }

        // POST: /Admin/AddEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddEvent(Event model)
        {
            if (!ModelState.IsValid)
                return View(model);

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
                cmd.Parameters.AddWithValue("@ImageUrl", string.IsNullOrEmpty(model.ImageUrl) ? "/images/default-concert.jpg" : model.ImageUrl);

                cmd.ExecuteNonQuery();

                TempData["Success"] = "✅ Event added successfully!";
                return RedirectToAction("AddEvent");
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error (AddEvent): " + ex.Message);
                ViewBag.Error = "❌ Failed to add event.";
                return View(model);
            }
        }
    }
}
