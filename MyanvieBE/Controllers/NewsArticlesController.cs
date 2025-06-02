// MyanvieBE/Controllers/NewsArticlesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyanvieBE.DTOs.News;
using MyanvieBE.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/news")] // Đường dẫn cơ sở là "api/news"
    public class NewsArticlesController : ControllerBase
    {
        private readonly INewsArticleService _newsArticleService;
        private readonly ILogger<NewsArticlesController> _logger;

        public NewsArticlesController(INewsArticleService newsArticleService, ILogger<NewsArticlesController> logger)
        {
            _newsArticleService = newsArticleService;
            _logger = logger;
        }

        // GET: api/news (Public)
        [HttpGet]
        public async Task<IActionResult> GetAllPublicArticles()
        {
            _logger.LogInformation("Endpoint GET /api/news called");
            var articles = await _newsArticleService.GetAllPublicArticlesAsync();
            return Ok(articles);
        }

        // GET: api/news/{id} (Public)
        [HttpGet("{id:guid}")] // Ràng buộc id phải là Guid
        public async Task<IActionResult> GetPublicArticleById(Guid id)
        {
            _logger.LogInformation("Endpoint GET /api/news/{ArticleId} called", id);
            var article = await _newsArticleService.GetPublicArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return Ok(article);
        }

        // GET: api/news/slug/{slug} (Public)
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetPublicArticleBySlug(string slug)
        {
            _logger.LogInformation("Endpoint GET /api/news/slug/{Slug} called", slug);
            var article = await _newsArticleService.GetPublicArticleBySlugAsync(slug);
            if (article == null)
            {
                return NotFound();
            }
            return Ok(article);
        }

        // POST: api/news (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được tạo bài viết
        public async Task<IActionResult> CreateArticle([FromBody] CreateNewsArticleDto createArticleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                 User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(authorIdString) || !Guid.TryParse(authorIdString, out Guid authorId))
            {
                _logger.LogWarning("CreateArticle: Author ID not found or invalid in token.");
                return Unauthorized(new { message = "Token không hợp lệ hoặc không chứa User ID hợp lệ." });
            }

            _logger.LogInformation("Endpoint POST /api/news called by Author ID: {AuthorId}", authorId);

            try
            {
                var createdArticle = await _newsArticleService.CreateArticleAsync(createArticleDto, authorId);
                return CreatedAtAction(nameof(GetPublicArticleById), new { id = createdArticle.Id }, createdArticle);
            }
            // catch (InvalidOperationException opEx) // Nếu bạn có kiểm tra slug tồn tại
            // {
            //     _logger.LogWarning(opEx, "Error creating article: Slug already exists or other operational error.");
            //     return Conflict(new { message = opEx.Message });
            // }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news article.");
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi tạo bài viết." });
            }
        }

        // PUT: api/news/{id} (Admin only)
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] CreateNewsArticleDto updateArticleDto)
        {
            var editorUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                     User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            // Sẽ không bao giờ null nếu [Authorize] hoạt động đúng, nhưng vẫn nên kiểm tra
            if (string.IsNullOrEmpty(editorUserIdString) || !Guid.TryParse(editorUserIdString, out Guid editorUserId))
            {
                _logger.LogWarning("UpdateArticle: Editor User ID not found or invalid in token for article {ArticleId}.", id);
                return Unauthorized(new { message = "Token không hợp lệ." });
            }

            _logger.LogInformation("Endpoint PUT /api/news/{ArticleId} called by User ID: {EditorUserId}", id, editorUserId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedArticle = await _newsArticleService.UpdateArticleAsync(id, updateArticleDto, editorUserId);
                if (updatedArticle == null)
                {
                    _logger.LogWarning("Admin: UpdateArticle failed for ID {ArticleId} or article not found.", id);
                    return NotFound(new { message = "Không tìm thấy bài viết hoặc không thể cập nhật." });
                }
                return Ok(updatedArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating news article ID {ArticleId}", id);
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi cập nhật bài viết." });
            }
        }

        // DELETE: api/news/{id} (Admin only)
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            var deleterUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                     User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            _logger.LogInformation("Endpoint DELETE /api/news/{ArticleId} called by User: {DeleterUserId}", id, deleterUserIdString);

            var success = await _newsArticleService.DeleteArticleAsync(id);
            if (!success)
            {
                _logger.LogWarning("Admin: DeleteArticle failed for ID {ArticleId} or article not found.", id);
                return NotFound(new { message = "Không tìm thấy bài viết hoặc không thể xóa." });
            }
            return NoContent(); // 204 No Content
        }
    }
}