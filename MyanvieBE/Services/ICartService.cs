// MyanvieBE/Services/ICartService.cs
using MyanvieBE.DTOs.Cart;

namespace MyanvieBE.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(Guid userId);
        Task<CartDto> AddToCartAsync(Guid userId, AddCartItemDto addToCartDto);
        Task<CartDto> UpdateCartItemAsync(Guid userId, UpdateCartItemDto updateCartItemDto);
        Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId);
        Task<bool> ClearCartAsync(Guid userId);
    }
}