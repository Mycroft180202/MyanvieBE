// MyanvieBE/Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyanvieBE.DTOs.Category;
using MyanvieBE.Services;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ISubCategoryService _subCategoryService; // Thêm service

        public CategoriesController(ICategoryService categoryService, ISubCategoryService subCategoryService)
        {
            _categoryService = categoryService;
            _subCategoryService = subCategoryService;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/categories/with-subcategories
        [HttpGet("with-subcategories")]
        public async Task<IActionResult> GetAllCategoriesWithSubCategories()
        {
            var categories = await _categoryService.GetAllCategoriesWithSubCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/categories/{id}/subcategories
        [HttpGet("{id}/subcategories")]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(Guid id)
        {
            var subCategories = await _subCategoryService.GetSubCategoriesByCategoryIdAsync(id);
            return Ok(subCategories);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdCategory = await _categoryService.CreateCategoryAsync(createCategoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
            if (updatedCategory == null)
                return NotFound();

            return Ok(updatedCategory);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                    return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}