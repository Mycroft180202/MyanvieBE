// MyanvieBE/Models/OrderItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class OrderItem : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Required]
        public Guid ProductId { get; set; } // Khóa ngoại trỏ tới bảng Products

        // ---> THÊM HOẶC SỬA LẠI THUỘC TÍNH NAVIGATION NÀY <---
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } // Thuộc tính navigation tới Product

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } // Giá tại thời điểm mua
    }
}