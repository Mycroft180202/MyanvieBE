// MyanvieBE/DTOs/Auth/VerifyResetCodeDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Auth
{
    public class VerifyResetCodeDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mã khôi phục là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã khôi phục phải có 6 chữ số")]
        public string Code { get; set; }
    }
}