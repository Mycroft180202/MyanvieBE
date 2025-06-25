// MyanvieBE/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyanvieBE.DTOs.Auth;
using MyanvieBE.Services;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
            {
                return BadRequest(new { message = "Email đã tồn tại." });
            }

            return Ok(new { message = "Đăng ký thành công.", user = result });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác." });
            }

            return Ok(result);
        }

        // POST: api/auth/request-password-reset
        [HttpPost("request-password-reset")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RequestPasswordResetAsync(dto.Email);
            if (!result)
            {
                // Trả về lỗi 500 nếu việc gửi mail thất bại
                return StatusCode(500, new { message = "Gửi email khôi phục thất bại, vui lòng thử lại." });
            }

            // Luôn trả về Ok để tránh lộ thông tin email có tồn tại trong hệ thống hay không
            return Ok(new { message = "Nếu email của bạn tồn tại trong hệ thống, chúng tôi đã gửi một mã khôi phục." });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(resetDto);
            if (!result)
            {
                return BadRequest(new { message = "Mã khôi phục không hợp lệ hoặc đã hết hạn." });
            }

            return Ok(new { message = "Mật khẩu đã được khôi phục thành công." });
        }


        [HttpPost("verify-reset-code")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto verifyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.VerifyPasswordResetCodeAsync(verifyDto);

            if (!result)
            {
                return BadRequest(new { message = "Mã xác thực không hợp lệ hoặc đã hết hạn." });
            }

            return Ok(new { message = "Mã xác thực hợp lệ." });
        }
    }
}