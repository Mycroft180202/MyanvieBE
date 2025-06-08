// MyanvieBE/DTOs/Cart/CartDto.cs
namespace MyanvieBE.DTOs.Cart
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemDto> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}