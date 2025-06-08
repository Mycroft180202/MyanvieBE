// MyanvieBE/DTOs/SubCategory/CreateSubCategoryDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.SubCategory
{
    public class CreateSubCategoryDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
    }
}