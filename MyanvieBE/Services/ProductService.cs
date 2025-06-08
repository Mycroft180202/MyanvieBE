// MyanvieBE/Services/ProductService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyanvieBE.Data;
using MyanvieBE.DTOs.Product;
using MyanvieBE.Models;

namespace MyanvieBE.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext context, IMapper mapper, ILogger<ProductService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("Getting all products");
            var products = await _context.Products
                .Include(p => p.SubCategory) // Thay đổi
                    .ThenInclude(sc => sc.Category) // Thay đổi
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", id);
            var product = await _context.Products
                .Include(p => p.SubCategory) // Thay đổi
                    .ThenInclude(sc => sc.Category) // Thay đổi
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return _mapper.Map<ProductDto?>(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            _logger.LogInformation("Creating a new product with name: {ProductName}", createProductDto.Name);

            // Thay đổi: Kiểm tra sự tồn tại của SubCategory
            var subCategoryExists = await _context.SubCategories.AnyAsync(c => c.Id == createProductDto.SubCategoryId);
            if (!subCategoryExists)
            {
                _logger.LogError("Invalid SubCategoryId: {SubCategoryId} provided.", createProductDto.SubCategoryId);
                throw new KeyNotFoundException($"SubCategory with ID {createProductDto.SubCategoryId} not found.");
            }

            var product = _mapper.Map<Product>(createProductDto);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

            // Load lại thông tin để trả về DTO đầy đủ
            await _context.Entry(product).Reference(p => p.SubCategory).LoadAsync();
            await _context.Entry(product.SubCategory).Reference(sc => sc.Category).LoadAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateProductAsync(Guid id, CreateProductDto updateProductDto)
        {
            _logger.LogInformation("Attempting to update product with ID: {ProductId}", id);

            var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (productInDb == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} NOT FOUND for update.", id);
                return null;
            }

            // Thay đổi: Kiểm tra SubCategory mới
            if (productInDb.SubCategoryId != updateProductDto.SubCategoryId)
            {
                var subCategoryExists = await _context.SubCategories.AnyAsync(c => c.Id == updateProductDto.SubCategoryId);
                if (!subCategoryExists)
                {
                    _logger.LogError("Invalid new SubCategoryId: {SubCategoryId} provided.", updateProductDto.SubCategoryId);
                    throw new KeyNotFoundException($"New SubCategory with ID {updateProductDto.SubCategoryId} not found.");
                }
            }

            _mapper.Map(updateProductDto, productInDb);
            productInDb.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Product with ID: {ProductId} updated successfully.", id);

            await _context.Entry(productInDb).Reference(p => p.SubCategory).LoadAsync();
            await _context.Entry(productInDb.SubCategory).Reference(sc => sc.Category).LoadAsync();
            return _mapper.Map<ProductDto>(productInDb);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            _logger.LogInformation("Attempting to delete product with ID: {ProductId}", id);
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for deletion.", id);
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product with ID: {ProductId} deleted successfully.", id);
            return true;
        }
    }
}