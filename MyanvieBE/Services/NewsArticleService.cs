// MyanvieBE/Services/NewsArticleService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyanvieBE.Data;
using MyanvieBE.DTOs.News;
using MyanvieBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<NewsArticleService> _logger;

        public NewsArticleService(ApplicationDbContext context, IMapper mapper, ILogger<NewsArticleService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<NewsArticleDto>> GetAllPublicArticlesAsync()
        {
            _logger.LogInformation("Fetching all public news articles.");
            var articles = await _context.NewsArticles
                .Where(a => a.Status == ArticleStatus.Published)
                .Include(a => a.Author) // Để lấy AuthorFullName
                .OrderByDescending(a => a.CreatedAt) // Hoặc UpdatedAt/PublishedDate
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<NewsArticleDto>>(articles);
        }

        public async Task<NewsArticleDto?> GetPublicArticleByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching public news article by ID: {ArticleId}", id);
            var article = await _context.NewsArticles
                .Include(a => a.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.Status == ArticleStatus.Published);

            if (article == null)
            {
                _logger.LogWarning("Public news article with ID {ArticleId} not found or not published.", id);
                return null;
            }
            return _mapper.Map<NewsArticleDto>(article);
        }

        public async Task<NewsArticleDto?> GetPublicArticleBySlugAsync(string slug)
        {
            _logger.LogInformation("Fetching public news article by Slug: {Slug}", slug);
            var article = await _context.NewsArticles
                .Include(a => a.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Slug == slug && a.Status == ArticleStatus.Published);

            if (article == null)
            {
                _logger.LogWarning("Public news article with Slug {Slug} not found or not published.", slug);
                return null;
            }
            return _mapper.Map<NewsArticleDto>(article);
        }

        public async Task<NewsArticleDto> CreateArticleAsync(CreateNewsArticleDto createArticleDto, Guid authorId)
        {
            _logger.LogInformation("Admin: Attempting to create news article with Title: {Title} by Author ID: {AuthorId}",
                createArticleDto.Title, authorId);

            // (Tùy chọn: Kiểm tra xem slug đã tồn tại chưa)
            // var slugExists = await _context.NewsArticles.AnyAsync(a => a.Slug == createArticleDto.Slug);
            // if (slugExists) { throw new InvalidOperationException("Slug đã tồn tại."); }

            var article = _mapper.Map<NewsArticle>(createArticleDto);
            article.AuthorId = authorId;
            // CreatedAt, UpdatedAt, Id sẽ được BaseEntity tự xử lý

            await _context.NewsArticles.AddAsync(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin: News article created successfully with ID: {ArticleId}", article.Id);

            // Load lại Author để map AuthorFullName cho DTO trả về
            await _context.Entry(article).Reference(a => a.Author).LoadAsync();
            return _mapper.Map<NewsArticleDto>(article);
        }

        public async Task<NewsArticleDto?> UpdateArticleAsync(Guid articleId, CreateNewsArticleDto updateArticleDto, Guid editorUserId)
        {
            _logger.LogInformation("Admin: Attempting to update news article ID: {ArticleId} by User ID: {EditorUserId}", articleId, editorUserId);

            var articleInDb = await _context.NewsArticles
                .Include(a => a.Author) // Include Author để khi map trả về DTO có AuthorFullName
                .FirstOrDefaultAsync(a => a.Id == articleId);

            if (articleInDb == null)
            {
                _logger.LogWarning("Admin: News article with ID {ArticleId} NOT FOUND for update.", articleId);
                return null;
            }

            // Map các thuộc tính từ DTO cập nhật vào entity đã được theo dõi
            // Các trường như AuthorId không nên được cập nhật qua DTO này (trừ khi có logic riêng)
            // MappingProfile của chúng ta đã Ignore AuthorId và Author khi map từ CreateNewsArticleDto
            _mapper.Map(updateArticleDto, articleInDb);

            articleInDb.UpdatedAt = DateTime.UtcNow;
            // Có thể bạn muốn lưu lại editorUserId nếu có trường EditorId trong NewsArticle model
            // articleInDb.LastEditorId = editorUserId;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin: News article with ID {ArticleId} updated successfully by User ID: {EditorUserId}", articleId, editorUserId);
                return _mapper.Map<NewsArticleDto>(articleInDb); // Trả về DTO của article đã cập nhật
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Admin: Concurrency Exception on UpdateArticle for ID: {ArticleId}", articleId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin: Generic Exception on SaveChanges for UpdateArticle, ID: {ArticleId}", articleId);
                return null;
            }
        }

        public async Task<bool> DeleteArticleAsync(Guid articleId)
        {
            _logger.LogInformation("Admin: Attempting to delete news article with ID: {ArticleId}", articleId);
            var article = await _context.NewsArticles.FindAsync(articleId);

            if (article == null)
            {
                _logger.LogWarning("Admin: News article with ID {ArticleId} not found for deletion.", articleId);
                return false;
            }

            _context.NewsArticles.Remove(article);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin: News article with ID {ArticleId} deleted successfully.", articleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin: Error deleting news article with ID: {ArticleId}", articleId);
                return false;
            }
        }
    }
}