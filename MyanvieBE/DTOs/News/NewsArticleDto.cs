// MyanvieBE/DTOs/News/NewsArticleDto.cs
using MyanvieBE.Models;

namespace MyanvieBE.DTOs.News
{
    public class NewsArticleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ThumbnailUrl { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorFullName { get; set; }
        public ArticleStatus Status { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}