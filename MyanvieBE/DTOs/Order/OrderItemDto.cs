// MyanvieBE/DTOs/Order/OrderItemDto.cs
namespace MyanvieBE.DTOs.Order
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductThumbnailUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}