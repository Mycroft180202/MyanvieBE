// MyanvieBE/DTOs/Auth/AdminUpdateUserDto.cs
using MyanvieBE.Models;
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Auth
{
    public class AdminUpdateUserDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public UserRole Role { get; set; }
    }
}