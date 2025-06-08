// MyanvieBE/Controllers/SubCategoriesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyanvieBE.DTOs.SubCategory;
using MyanvieBE.Services;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/subcategories")]
    public class SubCategoriesController : ControllerBase
    {
        private readonly ISubCategoryService _subCategoryService;

        public SubCategoriesController(ISubCategoryService subCategoryService)
        {
            _subCategoryService = subCategoryService;
        }

        // GET: api/subcategories
        [HttpGet]
        public async Task<IActionResult> GetAllSubCategories()
        {
            var subCategories = await _subCategoryService.GetAllSubCategoriesAsync();
            return Ok(subCategories);
        }

        // GET: api/subcategories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubCategoryById(Guid id)
        {
            var subCategory = await _subCategoryService.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }
            return Ok(subCategory);
        }

        // POST: api/subcategories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdSubCategory = await _subCategoryService.CreateSubCategoryAsync(createDto);
                return CreatedAtAction(nameof(GetSubCategoryById), new { id = createdSubCategory.Id }, createdSubCategory);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/subcategories/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubCategory(Guid id, [FromBody] CreateSubCategoryDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedSubCategory = await _subCategoryService.UpdateSubCategoryAsync(id, updateDto);
                if (updatedSubCategory == null)
                {
                    return NotFound();
                }
                return Ok(updatedSubCategory);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/subcategories/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubCategory(Guid id)
        {
            try
            {
                var success = await _subCategoryService.DeleteSubCategoryAsync(id);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}