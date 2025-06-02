// MyanvieBE/DTOs/Product/CreateProductVariantDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Product
{
    public class CreateProductVariantDto
    {
        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        [MaxLength(100)]
        public string? Sku { get; set; }
    }
}