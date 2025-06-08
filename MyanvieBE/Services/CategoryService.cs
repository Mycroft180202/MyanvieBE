// MyanvieBE/Services/CategoryService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyanvieBE.Data;
using MyanvieBE.DTOs.Category;
using MyanvieBE.Models;

namespace MyanvieBE.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<CategoryWithSubCategoriesDto>> GetAllCategoriesWithSubCategoriesAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<CategoryWithSubCategoriesDto>>(categories);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = _mapper.Map<Category>(createCategoryDto);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            return _mapper.Map<CategoryDto?>(category);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, CreateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return null;
            }

            _mapper.Map(updateCategoryDto, category);
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            // Cập nhật: Kiểm tra xem có SubCategory nào không
            var hasSubCategories = await _context.SubCategories.AnyAsync(sc => sc.CategoryId == id);
            if (hasSubCategories)
            {
                throw new InvalidOperationException("Không thể xóa danh mục cha khi vẫn còn danh mục con.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}