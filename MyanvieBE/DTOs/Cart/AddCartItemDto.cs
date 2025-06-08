// MyanvieBE/DTOs/Cart/AddCartItemDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Cart
{
    public class AddCartItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required, Range(1, 100)]
        public int Quantity { get; set; }
    }
}