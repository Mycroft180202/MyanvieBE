// MyanvieBE/Services/ISubCategoryService.cs
using MyanvieBE.DTOs.SubCategory;

namespace MyanvieBE.Services
{
    public interface ISubCategoryService
    {
        Task<IEnumerable<SubCategoryDto>> GetAllSubCategoriesAsync();
        Task<SubCategoryDto?> GetSubCategoryByIdAsync(Guid id);
        Task<IEnumerable<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(Guid categoryId);
        Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createDto);
        Task<SubCategoryDto?> UpdateSubCategoryAsync(Guid id, CreateSubCategoryDto updateDto);
        Task<bool> DeleteSubCategoryAsync(Guid id);
    }
}