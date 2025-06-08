// MyanvieBE/DTOs/Auth/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Auth
{
    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}