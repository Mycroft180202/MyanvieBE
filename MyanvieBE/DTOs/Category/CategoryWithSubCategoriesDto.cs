// MyanvieBE/DTOs/Category/CategoryWithSubCategoriesDto.cs
using MyanvieBE.DTOs.SubCategory;

namespace MyanvieBE.DTOs.Category
{
    public class CategoryWithSubCategoriesDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}