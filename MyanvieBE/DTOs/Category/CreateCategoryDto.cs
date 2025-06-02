// MyanvieBE/DTOs/Category/CreateCategoryDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Category
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}