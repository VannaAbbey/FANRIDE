using System.ComponentModel.DataAnnotations;

namespace FanRide.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Province { get; set; }

        public string PhoneNumber { get; set; } = null!; // ✅ ADD THIS


        [Required]
        public string Role { get; set; } // Admin, Rider, Driver
    }
}
