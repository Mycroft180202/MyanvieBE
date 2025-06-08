// MyanvieBE/Models/SubCategory.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class SubCategory : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }

        // Khóa ngoại tới Category (danh mục cha)
        [Required]
        public Guid CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        // Một danh mục con có nhiều sản phẩm
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}