// MyanvieBE/Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } // Ví dụ: "Khăn Lụa Hà Đông - Màu Đỏ - Size 90x90cm"

        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Foreign Key: sản phẩm thuộc về một danh mục
        [Required]
        public Guid CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        // Các trường từ ProductVariant cũ được đưa vào đây
        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        [MaxLength(100)]
        public string? Sku { get; set; } // Mã SKU cho sản phẩm/biến thể cụ thể này

        // Collection ProductReview vẫn giữ nguyên (nếu bạn muốn mỗi product cụ thể có review riêng)
        public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}