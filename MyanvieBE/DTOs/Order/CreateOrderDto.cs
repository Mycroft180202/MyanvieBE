// MyanvieBE/DTOs/Order/CreateOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; }

        // ---- THUỘC TÍNH BỔ SUNG ĐỂ SỬA LỖI ----
        [Required]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
        public List<CreateOrderItemDto> Items { get; set; }
    }
}