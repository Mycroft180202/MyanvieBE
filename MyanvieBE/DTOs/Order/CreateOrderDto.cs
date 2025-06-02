// MyanvieBE/DTOs/Order/CreateOrderDto.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        public string ShippingAddress { get; set; }

        // Có thể thêm các trường khác như Ghi chú, Số điện thoại nhận hàng (nếu khác profile)
        // public string? Notes { get; set; }
        // public string? ShippingPhoneNumber { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm")]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}