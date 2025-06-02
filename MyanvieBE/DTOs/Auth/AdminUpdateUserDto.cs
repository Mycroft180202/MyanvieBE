// MyanvieBE/DTOs/Auth/AdminUpdateUserDto.cs
using System.ComponentModel.DataAnnotations;
using MyanvieBE.Models; // Để dùng UserRole

namespace MyanvieBE.DTOs.Auth
{
    public class AdminUpdateUserDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(100)]
        public string FullName { get; set; }

        // Email thường không cho phép admin thay đổi trực tiếp vì nó là định danh đăng nhập.
        // Nếu cần thay đổi email, đó nên là một quy trình riêng.

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public UserRole Role { get; set; } // Admin có thể thay đổi vai trò
    }
}