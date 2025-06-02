// MyanvieBE/DTOs/Auth/LoginResponseDto.cs
namespace MyanvieBE.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; } 
    }
}