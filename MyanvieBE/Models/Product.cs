// MyanvieBE/Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } // Ví dụ: "Áo Lụa Tơ Tằm Vàng - Size M"

        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }

        [Required]
        public Guid SubCategoryId { get; set; } // Đổi từ CategoryId
        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; } // Đổi từ Category

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
        public string? Sku { get; set; }

        public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}