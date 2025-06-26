// MyanvieBE/DTOs/Order/CreateOrderDto.cs
using MyanvieBE.Models;
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Số điện thoại phải có 10 chữ số.")]
        public string CustomerPhone { get; set; }

        [Required]
        [EnumDataType(typeof(PaymentMethod))] 
        public PaymentMethod PaymentMethod { get; set; } 

        [Required]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
        public List<CreateOrderItemDto> Items { get; set; }
    }
}