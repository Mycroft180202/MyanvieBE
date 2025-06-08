// MyanvieBE/DTOs/Cart/UpdateCartItemDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Cart
{
    public class UpdateCartItemDto
    {
        [Required]
        public Guid CartItemId { get; set; }

        [Required]
        [Range(1, 100)] // Giả sử số lượng tối thiểu là 1
        public int Quantity { get; set; }
    }
}