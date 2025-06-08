// MyanvieBE/DTOs/News/CreateNewsArticleDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.News
{
    public class CreateNewsArticleDto
    {
        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string? ThumbnailUrl { get; set; }

        [Required, MaxLength(300)]
        public string Slug { get; set; }
    }
}