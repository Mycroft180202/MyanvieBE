// MyanvieBE/Models/Order.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class Order : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
       
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public long? PaymentTransactionId { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,      // Đang chờ xử lý
        Processing,   // Đang xử lý
        Shipped,      // Đã giao hàng
        Delivered,    // Đã nhận hàng
        Cancelled,    // Đã hủy
        Returned      // Trả hàng
    }
}