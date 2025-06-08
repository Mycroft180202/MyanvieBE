// MyanvieBE/DTOs/Order/CreateOrderItemDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class CreateOrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}