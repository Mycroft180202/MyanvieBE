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
            _logger.LogInformation("Getting all products (simplified model)");
            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting product by ID (simplified model): {ProductId}", id);
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID (simplified model): {ProductId} not found.", id);
                return null;
            }
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            _logger.LogInformation("Creating a new product (simplified model) with name: {ProductName}", createProductDto.Name);
            var product = _mapper.Map<Product>(createProductDto); // AutoMapper sẽ map cả các trường mới

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
            if (!categoryExists)
            {
                _logger.LogError("Invalid CategoryId: {CategoryId} provided for new product.", product.CategoryId);
                throw new KeyNotFoundException($"Category with ID {product.CategoryId} not found.");
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product (simplified model) created successfully with ID: {ProductId}", product.Id);

            // Load lại Category để map CategoryName
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateProductAsync(Guid id, CreateProductDto updateProductDto)
        {
            _logger.LogInformation("Attempting to update product (simplified model) with ID: {ProductId}", id);

            var productInDb = await _context.Products
                .Include(p => p.Category) // Include Category để map CategoryName và để EF theo dõi
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productInDb == null)
            {
                _logger.LogWarning("Product with ID (simplified model): {ProductId} NOT FOUND for update.", id);
                return null;
            }
            _logger.LogInformation("Product with ID (simplified model): {ProductId} FOUND. Name: {ProductName}. Proceeding with update.", id, productInDb.Name);

            if (productInDb.CategoryId != updateProductDto.CategoryId)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId);
                if (!categoryExists)
                {
                    _logger.LogError("Invalid new CategoryId: {CategoryId} provided for product update.", updateProductDto.CategoryId);
                    throw new KeyNotFoundException($"New Category with ID {updateProductDto.CategoryId} not found for product update.");
                }
            }

            // AutoMapper sẽ cập nhật tất cả các trường từ DTO vào productInDb
            _mapper.Map(updateProductDto, productInDb);
            productInDb.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(); // Bây giờ lệnh này sẽ đơn giản hơn rất nhiều
                _logger.LogInformation("Product with ID (simplified model): {ProductId} updated successfully.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency Exception on UpdateProduct (simplified model) for ID: {ProductId}.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Exception on SaveChanges for UpdateProduct (simplified model), ID: {ProductId}", id);
                return null;
            }

            return _mapper.Map<ProductDto>(productInDb);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            _logger.LogInformation("Attempting to delete product (simplified model) with ID: {ProductId}", id);
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID (simplified model): {ProductId} not found for deletion.", id);
                return false;
            }

            _context.Products.Remove(product);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product with ID (simplified model): {ProductId} deleted successfully.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product (simplified model) with ID: {ProductId}", id);
                return false;
            }
        }
    }
}