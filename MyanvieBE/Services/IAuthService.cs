// MyanvieBE/Services/IAuthService.cs
using MyanvieBE.DTOs.Auth;

namespace MyanvieBE.Services
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserDto?> GetMyProfileAsync();
        Task<IEnumerable<UserDto>> GetAllUsersAsync(); 
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> UpdateUserAsAdminAsync(Guid id, AdminUpdateUserDto updateUserDto); // <-- THÊM DÒNG NÀY
        Task<bool> DeleteUserAsAdminAsync(Guid id);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> VerifyPasswordResetCodeAsync(VerifyResetCodeDto verifyDto); // <-- THÊM DÒNG NÀY
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetDto);
    }
}