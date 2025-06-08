// MyanvieBE/Services/SubCategoryService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyanvieBE.Data;
using MyanvieBE.DTOs.SubCategory;
using MyanvieBE.Models;

namespace MyanvieBE.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SubCategoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubCategoryDto>> GetAllSubCategoriesAsync()
        {
            var subCategories = await _context.SubCategories
                .Include(sc => sc.Category)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<SubCategoryDto>>(subCategories);
        }

        public async Task<IEnumerable<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(Guid categoryId)
        {
            var subCategories = await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Include(sc => sc.Category)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<SubCategoryDto>>(subCategories);
        }

        public async Task<SubCategoryDto?> GetSubCategoryByIdAsync(Guid id)
        {
            var subCategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.Id == id);
            return _mapper.Map<SubCategoryDto>(subCategory);
        }

        public async Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createDto)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createDto.CategoryId);
            if (!categoryExists)
            {
                throw new KeyNotFoundException("Category không tồn tại.");
            }

            var subCategory = _mapper.Map<SubCategory>(createDto);
            await _context.SubCategories.AddAsync(subCategory);
            await _context.SaveChangesAsync();

            // Load lại để có CategoryName
            await _context.Entry(subCategory).Reference(sc => sc.Category).LoadAsync();
            return _mapper.Map<SubCategoryDto>(subCategory);
        }

        public async Task<SubCategoryDto?> UpdateSubCategoryAsync(Guid id, CreateSubCategoryDto updateDto)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
            {
                return null;
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateDto.CategoryId);
            if (!categoryExists)
            {
                throw new KeyNotFoundException("Category không tồn tại.");
            }

            _mapper.Map(updateDto, subCategory);
            subCategory.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _context.Entry(subCategory).Reference(sc => sc.Category).LoadAsync();
            return _mapper.Map<SubCategoryDto>(subCategory);
        }

        public async Task<bool> DeleteSubCategoryAsync(Guid id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
            {
                return false;
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.SubCategoryId == id);
            if (hasProducts)
            {
                throw new InvalidOperationException("Không thể xóa danh mục con khi vẫn còn sản phẩm.");
            }

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}