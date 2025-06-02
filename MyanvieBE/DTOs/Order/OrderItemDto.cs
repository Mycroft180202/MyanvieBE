// MyanvieBE/DTOs/Order/OrderItemDto.cs
using System;

namespace MyanvieBE.DTOs.Order
{
    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } // Lấy tên sản phẩm để hiển thị
        public string? ProductThumbnailUrl { get; set; } // Lấy ảnh sản phẩm
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Giá tại thời điểm mua
        public decimal SubTotal => Quantity * Price;
    }
}