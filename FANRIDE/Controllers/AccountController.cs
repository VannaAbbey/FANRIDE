using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using FanRide.Models;

namespace FanRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public AccountController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        public IActionResult Login() => View();
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string role = model.Role?.Trim().ToLower() switch
            {
                "admin" => "Admin",
                "driver" => "Driver",
                _ => "Rider"
            };

            var user = new User
            {
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Province = model.Province,
                PhoneNumber = model.PhoneNumber, // ✅ Add this in your User model
                Role = role
            };

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
                INSERT INTO Users 
                (FirstName, MiddleName, LastName, Email, PasswordHash, Province, PhoneNumber, Role)
                VALUES (@FirstName, @MiddleName, @LastName, @Email, @PasswordHash, @Province, @PhoneNumber, @Role);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@MiddleName", user.MiddleName ?? "");
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@Province", user.Province ?? "");
            cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
            cmd.Parameters.AddWithValue("@Role", user.Role);

            user.Id = Convert.ToInt32(cmd.ExecuteScalar());

            await SignInUser(user);

            return RedirectToRoleDashboard(user.Role);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM Users WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string storedHash = reader["PasswordHash"].ToString()!;
                string role = reader["Role"].ToString()!;
                string name = reader["FirstName"].ToString()!;
                string phone = reader["PhoneNumber"].ToString()!;
                int id = Convert.ToInt32(reader["Id"]);

                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                {
                    var user = new User
                    {
                        Id = id,
                        FirstName = name,
                        Email = email,
                        PhoneNumber = phone,
                        Role = role
                    };

                    await SignInUser(user);
                    return RedirectToRoleDashboard(role);
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email),
                new Claim("PhoneNumber", user.PhoneNumber ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        private IActionResult RedirectToRoleDashboard(string role)
        {
            switch (role.Trim().ToLower())
            {
                case "admin":
                    return RedirectToAction("Dashboard", "Admin");
                case "driver":
                    return RedirectToAction("Dashboard", "Drivers");
                default:
                    return RedirectToAction("Dashboard", "Riders");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
