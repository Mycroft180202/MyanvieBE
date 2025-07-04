﻿// MyanvieBE/DTOs/Product/CreateProductDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Product
{
    public class CreateProductDto
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        [Required]
        public Guid SubCategoryId { get; set; } // Đổi sang SubCategoryId
        [MaxLength(50)]
        public string? Color { get; set; }
        [MaxLength(50)]
        public string? Size { get; set; }
        [Required, Range(0, (double)decimal.MaxValue)]
        public decimal Price { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [MaxLength(100)]
        public string? Sku { get; set; }
    }
}