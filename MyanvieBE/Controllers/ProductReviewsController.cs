// MyanvieBE/Controllers/ProductReviewsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyanvieBE.DTOs.Review;
using MyanvieBE.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/reviews")] // Lồng vào đường dẫn của product
    public class ProductReviewsController : ControllerBase
    {
        private readonly IProductReviewService _reviewService;
        private readonly ILogger<ProductReviewsController> _logger;

        public ProductReviewsController(IProductReviewService reviewService, ILogger<ProductReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // GET: api/products/{productId}/reviews
        [HttpGet]
        public async Task<IActionResult> GetReviews(Guid productId)
        {
            _logger.LogInformation("Endpoint GET /api/products/{ProductId}/reviews called", productId);
            var reviews = await _reviewService.GetReviewsForProductAsync(productId);
            return Ok(reviews);
        }

        // POST: api/products/{productId}/reviews
        [HttpPost]
        [Authorize] // Chỉ user đã đăng nhập mới được thêm review
        public async Task<IActionResult> AddReview(Guid productId, [FromBody] CreateProductReviewDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                               User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("AddReview for Product {ProductId}: User ID not found or invalid in token.", productId);
                return Unauthorized(new { message = "Token không hợp lệ hoặc không chứa User ID." });
            }

            _logger.LogInformation("Endpoint POST /api/products/{ProductId}/reviews called by User ID: {UserId}", productId, userId);

            try
            {
                var createdReview = await _reviewService.AddReviewAsync(productId, reviewDto, userId);
                if (createdReview == null)
                {
                    // Lý do có thể là sản phẩm không tồn tại, hoặc user đã review sản phẩm này
                    _logger.LogWarning("Failed to add review for Product {ProductId} by User {UserId}. Service returned null.", productId, userId);
                    return BadRequest(new { message = "Không thể thêm đánh giá. Sản phẩm không tồn tại hoặc bạn đã đánh giá sản phẩm này." });
                }
                // Có thể trả về CreatedAtAction nếu có endpoint GetReviewById
                return Ok(createdReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for Product {ProductId} by User {UserId}", productId, userId);
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi thêm đánh giá." });
            }
        }
    }
}