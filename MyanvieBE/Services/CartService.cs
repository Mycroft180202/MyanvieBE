// MyanvieBE/Services/CartService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyanvieBE.Data;
using MyanvieBE.DTOs.Cart;
using MyanvieBE.Models;

namespace MyanvieBE.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CartService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            // Lấy giỏ hàng, kèm theo các CartItem của nó
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Nếu chưa có, tạo mới và LƯU NGAY LẬP TỨC
                cart = new Cart { UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<CartDto> AddToCartAsync(Guid userId, AddCartItemDto addToCartDto)
        {
            // 1. Lấy hoặc tạo giỏ hàng. Đối tượng 'cart' trả về được EF theo dõi và đã tồn tại trong DB.
            var cart = await GetOrCreateCartAsync(userId);

            // 2. Kiểm tra sản phẩm
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == addToCartDto.ProductId);
            if (product == null || product.Stock < addToCartDto.Quantity)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại hoặc không đủ hàng.");
            }

            // 3. Làm việc trực tiếp với collection đã được load của 'cart'
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addToCartDto.ProductId);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có, chỉ cần cập nhật Quantity. EF sẽ tự động theo dõi thay đổi này.
                existingItem.Quantity += addToCartDto.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Nếu là sản phẩm mới, tạo đối tượng CartItem một cách tường minh
                var newCartItem = new CartItem
                {
                    CartId = cart.Id, // Gán khoá ngoại một cách rõ ràng
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity
                };
                // Thêm đối tượng mới vào DbSet. EF sẽ biết đây là lệnh INSERT.
                _context.CartItems.Add(newCartItem);
            }

            // 4. Cập nhật thời gian cho giỏ hàng và lưu tất cả thay đổi
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 5. Trả về DTO sau khi đã hoàn tất
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartDto> UpdateCartItemAsync(Guid userId, UpdateCartItemDto updateCartItemDto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == updateCartItemDto.CartItemId && ci.CartId == cart.Id);

            if (cartItem == null)
            {
                throw new KeyNotFoundException("Sản phẩm không có trong giỏ hàng.");
            }

            if (updateCartItemDto.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);
                if (product == null || product.Stock < updateCartItemDto.Quantity)
                {
                    throw new InvalidOperationException("Không đủ hàng trong kho.");
                }
                cartItem.Quantity = updateCartItemDto.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);

            if (cartItem == null) return false;

            _context.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            if (!cart.CartItems.Any()) return true;

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // --- Dùng để trả về kết quả cuối cùng ---
        public async Task<CartDto> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Nếu không có cart (sau tất cả các bước trên), trả về DTO rỗng
            if (cart == null)
            {
                return new CartDto { UserId = userId, CartItems = new List<DTOs.Cart.CartItemDto>() };
            }

            return _mapper.Map<CartDto>(cart);
        }
    }
}