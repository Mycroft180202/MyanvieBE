// MyanvieBE/Controllers/ProductsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Thêm using cho ILogger
using MyanvieBE.DTOs.Product;
using MyanvieBE.Services;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            _logger.LogInformation("Endpoint GET /api/products called");
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            _logger.LogInformation("Endpoint GET /api/products/{ProductId} called", id);
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found by controller.", id);
                return NotFound(); // Trả về 404 Not Found nếu không tìm thấy
            }
            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            _logger.LogInformation("Endpoint POST /api/products called with product name: {ProductName}", createProductDto.Name);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateProduct: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var createdProduct = await _productService.CreateProductAsync(createProductDto);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (KeyNotFoundException knfEx) // Bắt lỗi nếu CategoryId không hợp lệ
            {
                _logger.LogWarning(knfEx, "Error creating product due to invalid CategoryId.");
                return BadRequest(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product.");
                return StatusCode(500, "Lỗi xảy ra trong quá trình tạo sản phẩm.");
            }
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] CreateProductDto updateProductDto)
        {
            _logger.LogInformation("Endpoint PUT /api/products/{ProductId} called", id);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for UpdateProduct ID {ProductId}: {@ModelState}", id, ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto);
                if (updatedProduct == null)
                {
                    _logger.LogWarning("Product with ID: {ProductId} not found for update by controller.", id);
                    return NotFound();
                }
                return Ok(updatedProduct);
            }
            catch (KeyNotFoundException knfEx) // Bắt lỗi nếu CategoryId không hợp lệ
            {
                _logger.LogWarning(knfEx, "Error updating product ID {ProductId} due to invalid CategoryId.", id);
                return BadRequest(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product ID {ProductId}.", id);
                return StatusCode(500, "Lỗi xảy ra trong quá trình cập nhật sản phẩm.");
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            _logger.LogInformation("Endpoint DELETE /api/products/{ProductId} called", id);
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
            {
                _logger.LogWarning("Product with ID: {ProductId} not found for deletion by controller.", id);
                return NotFound();
            }
            _logger.LogInformation("Product with ID: {ProductId} deleted successfully by controller.", id);
            return NoContent(); // Trả về 204 No Content khi xóa thành công
        }
    }
}