// MyanvieBE/Services/IProductService.cs
using MyanvieBE.DTOs.Product;

namespace MyanvieBE.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(Guid id, CreateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(Guid id);
    }
}