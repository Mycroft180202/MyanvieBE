// MyanvieBE/DTOs/Auth/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Auth
{
    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}