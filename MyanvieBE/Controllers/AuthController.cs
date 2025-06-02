// MyanvieBE/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyanvieBE.DTOs.Auth;
using MyanvieBE.Services;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Đường dẫn sẽ là "api/auth"
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")] // Đường dẫn cụ thể: "api/auth/register"
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("Register endpoint called for email: {Email}", registerDto.Email);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userDto = await _authService.RegisterAsync(registerDto);

            if (userDto == null)
            {
                // Email đã tồn tại hoặc lỗi khác từ service
                return Conflict(new { message = "Email đã được sử dụng hoặc có lỗi xảy ra." });
            }

            // Đăng ký thành công, trả về thông tin người dùng (không có mật khẩu)
            return Ok(userDto);
            // Hoặc có thể trả về CreatedAtAction nếu bạn có endpoint GetUserById
            // return CreatedAtAction(nameof(UsersController.GetUserById), "Users", new { id = userDto.Id }, userDto);
        }

        [HttpPost("login")] // Đường dẫn cụ thể: "api/auth/login"
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Login endpoint called for email: {Email}", loginDto.Email);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginResultToken = await _authService.LoginAsync(loginDto);

            if (loginResultToken == null)
            {
                _logger.LogWarning("Login attempt failed for {Email}.", loginDto.Email);
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác." });
            }

            // Trả về token (hiện tại là placeholder)
            return Ok(new { token = loginResultToken });
        }

        // GET: api/auth/me
        [HttpGet("me")]
        [Authorize] // <-- CHỈ NHỮNG USER ĐÃ ĐĂNG NHẬP (CÓ TOKEN HỢP LỆ) MỚI GỌI ĐƯỢC
        public async Task<IActionResult> GetMyProfile()
        {
            _logger.LogInformation("GetMyProfile endpoint called.");
            var userDto = await _authService.GetMyProfileAsync();
            if (userDto == null)
            {
                // Điều này có thể xảy ra nếu token hợp lệ nhưng user ID trong token không tìm thấy trong DB
                // Hoặc service không lấy được User ID từ context (rất hiếm nếu token hợp lệ)
                _logger.LogWarning("GetMyProfile: Could not retrieve user profile from service.");
                return NotFound(new { message = "Không thể lấy thông tin người dùng." });
            }
            return Ok(userDto);
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            _logger.LogInformation("Forgot password request received for email: {Email}", forgotPasswordDto.Email);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authService.RequestPasswordResetAsync(forgotPasswordDto.Email);

            if (success)
            {
                // Luôn trả về thông báo chung chung để bảo mật
                return Ok(new { message = "Nếu email của bạn tồn tại trong hệ thống, một mã khôi phục mật khẩu sẽ được xử lý." });
            }
            else
            {
                // Lỗi này thường là lỗi server khi lưu vào DB hoặc lỗi nghiêm trọng khi cố gửi mail
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã có lỗi xảy ra trong quá trình xử lý yêu cầu, vui lòng thử lại." });
            }
        }

        // POST: api/auth/verify-reset-code
        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto verifyDto)
        {
            _logger.LogInformation("Verify reset code request received for email: {Email}", verifyDto.Email);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isValid = await _authService.VerifyPasswordResetCodeAsync(verifyDto);
            if (!isValid)
            {
                return BadRequest(new { message = "Mã khôi phục không hợp lệ hoặc đã hết hạn." });
            }

            return Ok(new { message = "Mã khôi phục hợp lệ. Bạn có thể tiến hành đặt lại mật khẩu." });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            _logger.LogInformation("Reset password request received for email: {Email}", resetDto.Email);
            if (!ModelState.IsValid)
            {
                // Kiểm tra ConfirmNewPassword có khớp NewPassword không (do Compare attribute)
                return BadRequest(ModelState);
            }

            var success = await _authService.ResetPasswordAsync(resetDto);
            if (!success)
            {
                return BadRequest(new { message = "Không thể đặt lại mật khẩu. Mã không hợp lệ, đã hết hạn, hoặc có lỗi xảy ra." });
            }

            return Ok(new { message = "Mật khẩu của bạn đã được đặt lại thành công." });
        }
    }
}