// MyanvieBE/Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyanvieBE.DTOs.Auth;
using MyanvieBE.Services; // Sử dụng lại IAuthService
using System;
using System.Threading.Tasks;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Đường dẫn sẽ là "api/users"
    [Authorize(Roles = "Admin")] // <-- QUAN TRỌNG: Cả controller này chỉ Admin mới vào được
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService; // Tạm dùng IAuthService
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAuthService authService, ILogger<UsersController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation("Admin endpoint GET /api/users called by User: {User}", User?.Identity?.Name);
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            _logger.LogInformation("Admin endpoint GET /api/users/{UserId} called by User: {User}", id, User?.Identity?.Name);
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Admin: User with ID {UserId} not found by controller.", id);
                return NotFound();
            }
            return Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserDto updateUserDto)
        {
            _logger.LogInformation("Admin endpoint PUT /api/users/{UserId} called by User: {CallingUser}", id, User?.Identity?.Name);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedUser = await _authService.UpdateUserAsAdminAsync(id, updateUserDto);

            if (updatedUser == null)
            {
                // Lý do có thể là user không tồn tại, hoặc lỗi concurrency
                _logger.LogWarning("Admin: UpdateUser failed for ID {UserId} or user not found.", id);
                return NotFound(new { message = "Không tìm thấy người dùng hoặc không thể cập nhật." });
            }
            return Ok(updatedUser);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            _logger.LogInformation("Admin endpoint DELETE /api/users/{UserId} called by User: {CallingUser}", id, User?.Identity?.Name);
            var success = await _authService.DeleteUserAsAdminAsync(id);
            if (!success)
            {
                _logger.LogWarning("Admin: DeleteUser failed for ID {UserId} or user not found.", id);
                return NotFound(new { message = "Không tìm thấy người dùng hoặc không thể xóa." });
            }
            return NoContent(); // 204 No Content
        }
    }
}