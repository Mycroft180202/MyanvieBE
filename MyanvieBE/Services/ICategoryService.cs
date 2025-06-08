// MyanvieBE/Services/ICategoryService.cs
using MyanvieBE.DTOs.Category;

namespace MyanvieBE.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryWithSubCategoriesDto>> GetAllCategoriesWithSubCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto updateCategoryDto);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}