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

            var user = new User
            {
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Province = model.Province,
                Role = model.Role?.Trim().ToLower() == "admin" ? "Admin" : "Rider" // Default to Rider
            };

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
                INSERT INTO Users 
                (FirstName, MiddleName, LastName, Email, PasswordHash, Province, Role)
                VALUES (@FirstName, @MiddleName, @LastName, @Email, @PasswordHash, @Province, @Role);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@MiddleName", user.MiddleName ?? "");
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@Province", user.Province ?? "");
            cmd.Parameters.AddWithValue("@Role", user.Role);

            user.Id = Convert.ToInt32(cmd.ExecuteScalar()); // Get generated ID

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role == "Admin")
                return RedirectToAction("Dashboard", "Admin");
            else if (user.Role == "Rider")
                return RedirectToAction("Dashboard", "Riders");

            return RedirectToAction("Login");
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
                string storedHash = reader["PasswordHash"].ToString();
                string role = reader["Role"].ToString();
                string name = reader["FirstName"].ToString();
                int id = Convert.ToInt32(reader["Id"]);

                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                        new Claim(ClaimTypes.Name, name),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("Email", email)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Dashboard", "Admin");
                    else if (role.Equals("Rider", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Dashboard", "Riders");
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
