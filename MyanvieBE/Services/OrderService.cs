// MyanvieBE/Services/OrderService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyanvieBE.Data;
using MyanvieBE.DTOs.Order;
using MyanvieBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, IMapper mapper, ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto, Guid userId)
        {
            _logger.LogInformation("Attempting to create order for User ID: {UserId}", userId);

            // Sử dụng transaction để đảm bảo toàn vẹn dữ liệu
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for creating order.", userId);
                    await transaction.RollbackAsync();
                    return null;
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    Status = OrderStatus.Pending, // Trạng thái ban đầu
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var itemDto in createOrderDto.Items)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Product with ID {ProductId} not found.", itemDto.ProductId);
                        await transaction.RollbackAsync();
                        throw new KeyNotFoundException($"Sản phẩm với ID {itemDto.ProductId} không tồn tại.");
                    }

                    if (product.Stock < itemDto.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for Product ID {ProductId}. Requested: {Requested}, Available: {Available}",
                            itemDto.ProductId, itemDto.Quantity, product.Stock);
                        await transaction.RollbackAsync();
                        throw new InvalidOperationException($"Không đủ hàng cho sản phẩm '{product.Name}'. Tồn kho: {product.Stock}, Yêu cầu: {itemDto.Quantity}.");
                    }

                    // Giảm tồn kho
                    product.Stock -= itemDto.Quantity;
                    product.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian cho product

                    var orderItem = new OrderItem
                    {
                        Order = order, // EF Core sẽ tự gán OrderId
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        Price = product.Price // Lấy giá từ database tại thời điểm đặt hàng
                    };
                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.Price * orderItem.Quantity;
                }

                order.TotalAmount = totalAmount;
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync(); // Lưu Order, OrderItems, và cập nhật Product stock

                await transaction.CommitAsync(); // Hoàn tất transaction
                _logger.LogInformation("Order {OrderId} created successfully for User ID: {UserId}", order.Id, userId);

                // Load lại đầy đủ thông tin để trả về DTO
                var createdOrderWithDetails = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product) // Để lấy ProductName, ProductThumbnailUrl
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                return _mapper.Map<OrderDto>(createdOrderWithDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for User ID: {UserId}", userId);
                await transaction.RollbackAsync();
                throw; // Ném lại exception để controller có thể bắt và trả về lỗi 500
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Admin: Getting all orders.");
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            _logger.LogInformation("Admin: Attempting to update status for Order ID: {OrderId} to {NewStatus}", orderId, newStatus);

            var order = await _context.Orders
                .Include(o => o.User) // Include để khi map trả về DTO có đủ thông tin
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Admin: Order with ID {OrderId} not found for status update.", orderId);
                return null;
            }

            // Có thể thêm logic kiểm tra việc chuyển đổi trạng thái có hợp lệ không ở đây
            // Ví dụ: không cho chuyển từ Delivered về Pending.
            // if (order.Status == OrderStatus.Delivered && newStatus == OrderStatus.Pending) { /* throw error */ }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin: Status for Order ID {OrderId} updated successfully to {NewStatus}", orderId, newStatus);
                return _mapper.Map<OrderDto>(order);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Admin: Concurrency Exception on UpdateOrderStatus for Order ID: {OrderId}", orderId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin: Generic Exception on SaveChanges for UpdateOrderStatus, Order ID: {OrderId}", orderId);
                return null;
            }
        }
        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId)
        {
            _logger.LogInformation("Fetching orders for User ID: {UserId}", userId);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin)
        {
            _logger.LogInformation("Fetching order by ID: {OrderId} for User ID: {UserId}. IsAdmin: {IsAdmin}", orderId, userId, isAdmin);

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                return null;
            }

            if (!isAdmin && order.UserId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to view Order {OrderId}.", userId, orderId);
                return null;
            }

            return _mapper.Map<OrderDto>(order);
        }
    }
}