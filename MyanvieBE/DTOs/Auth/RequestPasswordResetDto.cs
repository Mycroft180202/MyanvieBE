// MyanvieBE/DTOs/Auth/RequestPasswordResetDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Auth
{
    public class RequestPasswordResetDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}