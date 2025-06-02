// MyanvieBE/DTOs/News/CreateNewsArticleDto.cs
using System.ComponentModel.DataAnnotations;
using MyanvieBE.Models; // Để dùng ArticleStatus

namespace MyanvieBE.DTOs.News
{
    public class CreateNewsArticleDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        public string Content { get; set; }

        public string? ThumbnailUrl { get; set; } // Có thể là URL từ MinIO sau này

        [Required(ErrorMessage = "Slug là bắt buộc")]
        [MaxLength(300)]
        // TODO: Thêm regex để validate slug (ví dụ: chỉ chữ thường, số, dấu gạch ngang)
        public string Slug { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [EnumDataType(typeof(ArticleStatus))]
        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    }
}