// MyanvieBE/DTOs/Cart/CartItemDto.cs
namespace MyanvieBE.DTOs.Cart
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
    }
}