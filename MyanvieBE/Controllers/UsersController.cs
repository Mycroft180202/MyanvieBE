// MyanvieBE/Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyanvieBE.DTOs.Auth;
using MyanvieBE.Services;
using System.Security.Claims;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userProfile = await _authService.GetMyProfileAsync();
            if (userProfile == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });
            }
            return Ok(userProfile);
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedUser = await _authService.UpdateUserAsAdminAsync(id, updateUserDto);
            if (updatedUser == null)
            {
                return NotFound();
            }
            return Ok(updatedUser);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _authService.DeleteUserAsAdminAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy người dùng để xóa." });
            }
            return NoContent();
        }
    }
}