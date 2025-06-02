// MyanvieBE/DTOs/Product/ProductVariantDto.cs
namespace MyanvieBE.DTOs.Product
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}