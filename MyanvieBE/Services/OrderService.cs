using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyanvieBE.Data;
using MyanvieBE.DTOs.Order;
using MyanvieBE.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using VNPAY.NET;
using VNPAY.NET.Models;

namespace MyanvieBE.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IVnpay _vnpay; 
        private readonly IConfiguration _configuration;

        public OrderService(
            ApplicationDbContext context, 
            IMapper mapper,
            ILogger<OrderService> logger,
            IVnpay vnpay, 
            IConfiguration configuration
            )
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _vnpay = vnpay; 
            _configuration = configuration; 
        }

        public async Task<CreateOrderResponseDto?> CreateOrderAsync(CreateOrderDto createOrderDto, Guid userId)
        {
            _logger.LogInformation("Creating order for User ID: {UserId} with PaymentMethod: {PaymentMethod}", userId, createOrderDto.PaymentMethod);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    return null;
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    Status = OrderStatus.Pending,
                    PaymentMethod = createOrderDto.PaymentMethod,
                    OrderItems = new List<OrderItem>()
                };

                if (order.PaymentMethod == PaymentMethod.Vnpay)
                {
                    order.PaymentTransactionId = DateTime.UtcNow.Ticks;
                }

                decimal totalAmount = 0;
                foreach (var itemDto in createOrderDto.Items)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null || product.Stock < itemDto.Quantity)
                    {
                        await transaction.RollbackAsync();
                        throw new InvalidOperationException($"Sản phẩm ID {itemDto.ProductId} không tồn tại hoặc không đủ hàng.");
                    }
                    product.Stock -= itemDto.Quantity;
                    var orderItem = new OrderItem
                    {
                        Order = order,
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        Price = product.Price
                    };
                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.Price * orderItem.Quantity;
                }
                order.TotalAmount = totalAmount;

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                var response = new CreateOrderResponseDto();

                if (order.PaymentMethod == PaymentMethod.Vnpay && order.PaymentTransactionId.HasValue)
                {
                    _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);
                    var paymentRequest = new VNPAY.NET.Models.PaymentRequest
                    {
                        PaymentId = order.PaymentTransactionId.Value,
                        Money = (double)order.TotalAmount,
                        Description = $"Thanh toan don hang {order.Id}",
                        CreatedDate = order.CreatedAt,
                        IpAddress = "127.0.0.1" // Note: Nên lấy IP thực tế từ HttpContext
                    };
                    response.PaymentUrl = _vnpay.GetPaymentUrl(paymentRequest);
                    _logger.LogInformation("VNPay URL created for Order {OrderId}", order.Id);
                }

                await transaction.CommitAsync();

                var createdOrderWithDetails = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.User)
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                response.Order = _mapper.Map<OrderDto>(createdOrderWithDetails);

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order for User ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ProcessVnpayPaymentAsync(IQueryCollection vnpayResponse)
        {
            // === BẮT ĐẦU PHIÊN BẢN DEBUG ===
            _logger.LogInformation("--- BEGIN VNPay Callback Processing ---");
            _logger.LogInformation("Raw Vnpay Response Query: {query}", vnpayResponse.ToString());

            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);
            var paymentResult = _vnpay.GetPaymentResult(vnpayResponse);

            _logger.LogInformation("DEBUG: PaymentResult.IsSuccess = {IsSuccess}", paymentResult.IsSuccess);
            _logger.LogInformation("DEBUG: PaymentResult.PaymentId (vnp_TxnRef) = {PaymentId}", paymentResult.PaymentId);
            _logger.LogInformation("DEBUG: PaymentResult.ResponseCode = {ResponseCode}", paymentResult.PaymentResponse?.Code.ToString());

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.PaymentTransactionId == paymentResult.PaymentId);

            if (order == null)
            {
                _logger.LogWarning("DEBUG: Order with PaymentTransactionId {TxnId} NOT FOUND in database.", paymentResult.PaymentId);
                _logger.LogInformation("--- END VNPay Callback Processing (Order not found) ---");
                return true;
            }

            _logger.LogInformation("DEBUG: Found Order. OrderId = {OrderId}, Status = {Status}, TotalAmount = {TotalAmount}", order.Id, order.Status, order.TotalAmount);

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("DEBUG: Order {OrderId} has already been processed. Skipping update.", order.Id);
                _logger.LogInformation("--- END VNPay Callback Processing (Already processed) ---");
                return true;
            }

            if (paymentResult.IsSuccess)
            {
                if (!long.TryParse(vnpayResponse["vnp_Amount"], out var vnpayAmountRaw))
                {
                    _logger.LogError("Could not parse vnp_Amount from VNPay callback for Order {OrderId}", order.Id);
                    order.Status = OrderStatus.Cancelled;
                    await _context.SaveChangesAsync();
                    return false;
                }
                var vnpayAmount = (decimal)vnpayAmountRaw / 100;
                _logger.LogInformation("DEBUG: Amount check. DB Amount = {dbAmount}, VNPay Amount = {vnpayAmount}", order.TotalAmount, vnpayAmount);

                if (order.TotalAmount != vnpayAmount)
                {
                    _logger.LogWarning("DEBUG: VNPay amount MISMATCH for Order {OrderId}.", order.Id);
                    order.Status = OrderStatus.Cancelled;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("--- END VNPay Callback Processing (Amount mismatch) ---");
                    return false;
                }

                order.Status = OrderStatus.Processing;
                await _context.SaveChangesAsync();
                _logger.LogInformation("DEBUG: Successfully updated Order {OrderId} status to Processing.", order.Id);
                _logger.LogInformation("--- END VNPay Callback Processing (Success) ---");
                return true;
            }
            else
            {
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                _logger.LogWarning("DEBUG: VNPay payment failed. Updated Order {OrderId} status to Cancelled.", order.Id);
                _logger.LogInformation("--- END VNPay Callback Processing (VNPay failure) ---");
                return false;
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