// MyanvieBE/DTOs/Order/CreateOrderItemDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class CreateOrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; } // ID của Product (đã được đơn giản hóa)

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        // Giá sẽ được lấy từ database tại thời điểm tạo đơn hàng, không phải từ client
    }
}