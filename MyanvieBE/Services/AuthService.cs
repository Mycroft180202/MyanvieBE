// MyanvieBE/Services/AuthService.cs
using AutoMapper;
using Microsoft.AspNetCore.Http; // Cần cho IHttpContextAccessor
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyanvieBE.Data;
using MyanvieBE.DTOs;
using MyanvieBE.DTOs.Auth;
using MyanvieBE.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService; // Quan trọng: Inject IEmailService

        public AuthService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService) // Thêm IEmailService vào constructor
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService; // Gán giá trị
        }

        public async Task<UserDto?> RegisterAsync(RegisterDto registerDto)
        {
            _logger.LogInformation("Attempting to register new user with email: {Email}", registerDto.Email);
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists.", registerDto.Email);
                return null;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            DateTime? dateOfBirthUtc = null;
            if (registerDto.DateOfBirth.HasValue)
            {
                dateOfBirthUtc = registerDto.DateOfBirth.Value.ToUniversalTime();
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = System.Text.Encoding.UTF8.GetBytes(passwordHash),
                PasswordSalt = new byte[0], // BCrypt đã bao gồm salt trong hash
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                DateOfBirth = dateOfBirthUtc,
                Role = UserRole.Customer
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Attempting to login user with email: {Email}", loginDto.Email);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found.", loginDto.Email);
                return null;
            }

            string storedPasswordHashString = System.Text.Encoding.UTF8.GetString(user.PasswordHash);
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, storedPasswordHashString);

            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for user with email {Email}.", loginDto.Email);
                return null;
            }

            _logger.LogInformation("User {Email} logged in successfully.", loginDto.Email);
            string tokenString = GenerateJwtToken(user);
            var userDto = _mapper.Map<UserDto>(user);

            return new LoginResponseDto
            {
                Token = tokenString,
                User = userDto
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var securityKeyString = jwtSettings["Key"];
            if (string.IsNullOrEmpty(securityKeyString))
            {
                _logger.LogError("JWT Key is missing in configuration.");
                throw new InvalidOperationException("JWT Key is missing in configuration.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKeyString));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<UserDto?> GetMyProfileAsync()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                               _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("GetMyProfileAsync: User ID not found or invalid in token claims.");
                return null;
            }

            _logger.LogInformation("GetMyProfileAsync: Attempting to get profile for User ID: {UserId}", userId);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("GetMyProfileAsync: User with ID {UserId} not found in database.", userId);
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Admin: Getting all users.");
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            _logger.LogInformation("Admin: Getting user by ID: {UserId}", id);
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("Admin: User with ID {UserId} not found.", id);
                return null;
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateUserAsAdminAsync(Guid id, AdminUpdateUserDto updateUserDto)
        {
            _logger.LogInformation("Admin: Attempting to update user with ID: {UserId}", id);
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userInDb == null)
            {
                _logger.LogWarning("Admin: User with ID {UserId} not found for update.", id);
                return null;
            }

            _mapper.Map(updateUserDto, userInDb);
            userInDb.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin: User with ID {UserId} updated successfully.", id);
                return _mapper.Map<UserDto>(userInDb);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Admin: Concurrency Exception on UpdateUser for ID: {UserId}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin: Generic Exception on SaveChanges for UpdateUser, ID: {UserId}", id);
                return null;
            }
        }

        public async Task<bool> DeleteUserAsAdminAsync(Guid id)
        {
            _logger.LogInformation("Admin: Attempting to delete user with ID: {UserId}", id);
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                _logger.LogWarning("Admin: User with ID {UserId} not found for deletion.", id);
                return false;
            }

            _context.Users.Remove(user);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin: User with ID {UserId} deleted successfully.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin: Error deleting user with ID: {UserId}", id);
                return false;
            }
        }

        // PHIÊN BẢN RequestPasswordResetAsync VỚI LOGGING CHI TIẾT
        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            _logger.LogInformation("--- START: RequestPasswordResetAsync for email: {Email} ---", email);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email} during password reset request. No email will be sent.", email);
                _logger.LogInformation("--- END: RequestPasswordResetAsync for email: {Email} (User not found) ---", email);
                return true; // Vẫn trả về true để client không biết email có tồn tại không.
            }
            _logger.LogInformation("User found for email: {Email}. User ID: {UserId}", email, user.Id);

            var random = new Random();
            var resetCode = random.Next(100000, 999999).ToString();
            _logger.LogInformation("Generated reset code {ResetCode} for {Email}", resetCode, email);

            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("User entity updated with reset code and expiry for {Email}.", email);

            try
            {
                _logger.LogInformation("Attempting to SaveChangesAsync for user {Email} with reset code.", email);
                await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync successful for user {Email} (reset code saved to DB).", email);

                if (_emailService == null)
                {
                    _logger.LogCritical("_emailService IS NULL. Dependency Injection for IEmailService might have failed in Program.cs or AuthService constructor.");
                    _logger.LogInformation("--- END: RequestPasswordResetAsync for email: {Email} (EmailService is null) ---", email);
                    return false;
                }
                _logger.LogInformation("_emailService is NOT NULL. Preparing to call SendEmailAsync for {Email}.", email);

                var subject = "Myanvie - Mã Khôi Phục Mật Khẩu";
                var messageBody = $@"
                    <p>Chào {user.FullName ?? "bạn"},</p>
                    <p>Bạn đã yêu cầu khôi phục mật khẩu cho tài khoản Myanvie của mình.</p>
                    <p>Mã khôi phục của bạn là: <strong>{resetCode}</strong></p>
                    <p>Mã này sẽ hết hạn sau 15 phút.</p>
                    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                    <p>Trân trọng,<br/>Đội ngũ Myanvie</p>";

                _logger.LogInformation("CALLING _emailService.SendEmailAsync for {Email} NOW...", email);
                await _emailService.SendEmailAsync(user.Email, subject, messageBody);
                _logger.LogInformation("COMPLETED call to _emailService.SendEmailAsync for {Email}. Check EmailService logs for actual send status.", email);

                _logger.LogInformation("--- END: RequestPasswordResetAsync for email: {Email} (Email supposedly processed) ---", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPTION caught in RequestPasswordResetAsync's try-catch block for email {Email}. Exception Type: {ExceptionType}, Message: {ExceptionMessage}",
                                 email, ex.GetType().Name, ex.Message);
                _logger.LogInformation("--- END: RequestPasswordResetAsync for email: {Email} (Exception occurred) ---", email);
                return false;
            }
        }

        public async Task<bool> VerifyPasswordResetCodeAsync(VerifyResetCodeDto verifyDto)
        {
            _logger.LogInformation("Attempting to verify password reset code for email: {Email}", verifyDto.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == verifyDto.Email);

            if (user == null ||
                user.PasswordResetCode == null ||
                user.PasswordResetCodeExpiresAt == null ||
                user.PasswordResetCodeExpiresAt < DateTime.UtcNow ||
                user.PasswordResetCode != verifyDto.Code)
            {
                _logger.LogWarning("Password reset code verification failed for email: {Email}. Code provided: {Code}", verifyDto.Email, verifyDto.Code);
                return false;
            }

            _logger.LogInformation("Password reset code verified successfully for email: {Email}", verifyDto.Email);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            _logger.LogInformation("Attempting to reset password for email: {Email}", resetDto.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetDto.Email);

            if (user == null ||
                user.PasswordResetCode == null ||
                user.PasswordResetCodeExpiresAt == null ||
                user.PasswordResetCodeExpiresAt < DateTime.UtcNow ||
                user.PasswordResetCode != resetDto.Code)
            {
                _logger.LogWarning("Password reset failed for email: {Email}. Invalid user, code, or code expired. Code provided: {Code}",
                                   resetDto.Email, resetDto.Code);
                return false;
            }

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
            user.PasswordHash = System.Text.Encoding.UTF8.GetBytes(newPasswordHash);
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Password reset successfully for email: {Email}", resetDto.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving new password for email: {Email}", resetDto.Email);
                return false;
            }
        }
    }
}