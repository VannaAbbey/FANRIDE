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
                Role = model.Role ?? "Rider"
            };

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
                INSERT INTO Users 
                (FirstName, MiddleName, LastName, Email, PasswordHash, Province, Role)
                VALUES (@FirstName, @MiddleName, @LastName, @Email, @PasswordHash, @Province, @Role)", conn);

            cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
            cmd.Parameters.AddWithValue("@MiddleName", user.MiddleName ?? "");
            cmd.Parameters.AddWithValue("@LastName", user.LastName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@Province", user.Province ?? "");
            cmd.Parameters.AddWithValue("@Role", user.Role);

            cmd.ExecuteNonQuery();

            // Auto-login after registration
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect based on role
            if (user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Admin");
            else if (user.Role.Equals("rider", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Riders");
            // ✅ fixed for Razor Pages
            else
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

                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, name),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("Email", email)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Redirect based on role
                    if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Dashboard", "Admin");
                    else if (role.Equals("rider", StringComparison.OrdinalIgnoreCase))
                        return RedirectToAction("Dashboard", "Riders");
                    // ✅ fixed for Razor Pages
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
