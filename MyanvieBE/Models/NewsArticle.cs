// MyanvieBE/Models/NewsArticle.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class NewsArticle : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; } // Nội dung bài viết

        public string? ThumbnailUrl { get; set; } // Ảnh đại diện cho bài viết

        [Required]
        public Guid AuthorId { get; set; } // Khóa ngoại tới người viết bài
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }

        public ArticleStatus Status { get; set; } = ArticleStatus.Draft; // Mặc định là bản nháp

        [Required]
        [MaxLength(300)]
        public string Slug { get; set; } // Dùng để tạo URL thân thiện, ví dụ: /tin-tuc/bai-viet-moi-nhat
    }

    public enum ArticleStatus
    {
        Draft,     // Bản nháp
        Published  // Đã xuất bản
    }
}