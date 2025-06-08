// MyanvieBE/Controllers/ProductsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdProduct = await _productService.CreateProductAsync(createProductDto);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (KeyNotFoundException knfEx)
            {
                return BadRequest(new { message = knfEx.Message });
            }
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] CreateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto);
                if (updatedProduct == null)
                {
                    return NotFound();
                }
                return Ok(updatedProduct);
            }
            catch (KeyNotFoundException knfEx)
            {
                return BadRequest(new { message = knfEx.Message });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}