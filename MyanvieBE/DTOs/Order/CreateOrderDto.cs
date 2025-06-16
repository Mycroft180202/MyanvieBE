// MyanvieBE/DTOs/Order/CreateOrderDto.cs
using MyanvieBE.Models;
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        [EnumDataType(typeof(PaymentMethod))] 
        public PaymentMethod PaymentMethod { get; set; } 

        [Required]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
        public List<CreateOrderItemDto> Items { get; set; }
    }
}