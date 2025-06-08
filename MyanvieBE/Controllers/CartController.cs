// MyanvieBE/Controllers/CartController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyanvieBE.DTOs.Cart;
using MyanvieBE.Services;
using System.Security.Claims;

namespace MyanvieBE.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out userId))
            {
                return false;
            }
            return true;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddCartItemDto addToCartDto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var cart = await _cartService.AddToCartAsync(userId, addToCartDto);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto updateCartItemDto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var cart = await _cartService.UpdateCartItemAsync(userId, updateCartItemDto);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}