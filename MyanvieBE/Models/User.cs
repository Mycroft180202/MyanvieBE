using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.Models
{
    public class User : BaseEntity
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } // Sẽ dùng để đăng nhập

        public string? PhoneNumber { get; set; }

        // Không lưu mật khẩu trực tiếp
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }

        public string? Address { get; set; }

        public string? PasswordResetCode { get; set; } // Lưu mã 6 số
        public DateTime? PasswordResetCodeExpiresAt { get; set; } // Thời gian mã hết hạn

        public DateTime? DateOfBirth { get; set; } // Để tính toán độ tuổi

        public UserRole Role { get; set; } = UserRole.Customer; // Mặc định là khách hàng
    }

    public enum UserRole
    {
        Customer, // Khách hàng
        Admin     // Quản trị viên
    }
}