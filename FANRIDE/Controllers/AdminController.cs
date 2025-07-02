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
        private readonly string _connectionString = "server=localhost;database=fanride_db;user=root;password=Web123;";

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


    }
}
