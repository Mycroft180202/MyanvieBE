// MyanvieBE/Controllers/FilesController.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyanvieBE.Services;
using System;
using System.Threading.Tasks;

namespace MyanvieBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IMinioService _minioService;
        private readonly ILogger<FilesController> _logger;
        private readonly string _defaultBucketName;

        public FilesController(IMinioService minioService, IConfiguration configuration, ILogger<FilesController> logger)
        {
            _minioService = minioService;
            _logger = logger;
            _defaultBucketName = configuration["MinioSettings:BucketName"] ?? "default-bucket";
        }

        [HttpPost("upload")]
        // Có thể thêm [Authorize(Roles = "Admin")] ở đây để chỉ Admin được upload
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không có file nào được chọn hoặc file rỗng." });
            }

            // Có thể thêm kiểm tra kích thước file, loại file ở đây
            // Ví dụ: if (file.Length > 5 * 1024 * 1024) return BadRequest("Kích thước file quá lớn (tối đa 5MB).");
            // Ví dụ: if (!file.ContentType.StartsWith("image/")) return BadRequest("Chỉ chấp nhận file ảnh.");

            try
            {
                var fileUrl = await _minioService.UploadFileAsync(file, _defaultBucketName);
                _logger.LogInformation("File uploaded successfully. URL: {FileUrl}", fileUrl);
                return Ok(new { FileUrl = fileUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình upload file.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Đã có lỗi xảy ra khi upload file." });
            }
        }
    }
}