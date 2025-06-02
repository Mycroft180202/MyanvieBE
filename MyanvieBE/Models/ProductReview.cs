// MyanvieBE/Models/ProductReview.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class ProductReview : BaseEntity
    {
        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required, Range(1, 5)] // Đánh giá từ 1 đến 5 sao
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}