// MyanvieBE/Services/INewsArticleService.cs
// ... (các using và các phương thức khác đã có)
using MyanvieBE.DTOs.News;

namespace MyanvieBE.Services
{
    public interface INewsArticleService
    {
        Task<IEnumerable<NewsArticleDto>> GetAllPublicArticlesAsync();
        Task<NewsArticleDto?> GetPublicArticleByIdAsync(Guid id);
        Task<NewsArticleDto?> GetPublicArticleBySlugAsync(string slug);
        Task<NewsArticleDto> CreateArticleAsync(CreateNewsArticleDto createArticleDto, Guid authorId);
        Task<NewsArticleDto?> UpdateArticleAsync(Guid articleId, CreateNewsArticleDto updateArticleDto, Guid editorUserId); // <-- THÊM DÒNG NÀY
        Task<bool> DeleteArticleAsync(Guid articleId); // <-- THÊM DÒNG NÀY
    }
}