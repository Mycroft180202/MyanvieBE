using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyanvieBE.Data;
using MyanvieBE.DTOs.Order;
using MyanvieBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VNPAY.NET;
using VNPAY.NET.Models;
using Net.payOS;
using Net.payOS.Types;
using System.Text.Json;

namespace MyanvieBE.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly PayOS _payOS;
        private const decimal SHIPPING_FEE = 30000; // Fixed shipping fee of 30,000

        public OrderService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<OrderService> logger,
            IVnpay vnpay,
            IConfiguration configuration,
            PayOS payOS
            )
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _vnpay = vnpay;
            _configuration = configuration;
            _payOS = payOS;

            if (string.IsNullOrEmpty(configuration["PayOS:ClientId"]))
            {
                throw new ArgumentNullException(nameof(configuration), "PayOS:ClientId is missing in configuration.");
            }

            if (string.IsNullOrEmpty(configuration["PayOS:ApiKey"]))
            {
                throw new ArgumentNullException(nameof(configuration), "PayOS:ApiKey is missing in configuration.");
            }

            if (string.IsNullOrEmpty(configuration["PayOS:ChecksumKey"]))
            {
                throw new ArgumentNullException(nameof(configuration), "PayOS:ChecksumKey is missing in configuration.");
            }

            _payOS = new PayOS(
                configuration["PayOS:ClientId"],
                configuration["PayOS:ApiKey"],
                configuration["PayOS:ChecksumKey"]
            );
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

                if (order.PaymentMethod == PaymentMethod.Vnpay || order.PaymentMethod == PaymentMethod.PayOS)
                {
                    long unixTimestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    order.PaymentTransactionId = unixTimestamp;
                }

                decimal totalAmount = 0;
                var payOSItems = new List<ItemData>();
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

                    payOSItems.Add(new ItemData(product.Name, itemDto.Quantity, (int)product.Price));
                }
                // Add fixed shipping fee to total amount
                totalAmount += SHIPPING_FEE;
                order.TotalAmount = totalAmount;

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                var response = new CreateOrderResponseDto();
                string? paymentUrl = null;

                if (order.PaymentMethod == PaymentMethod.Vnpay && order.PaymentTransactionId.HasValue)
                {
                    _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);
                    var paymentRequest = new VNPAY.NET.Models.PaymentRequest
                    {
                        PaymentId = order.PaymentTransactionId.Value,
                        Money = (double)order.TotalAmount, // Includes shipping fee
                        Description = $"Thanh toan don hang {order.Id} (bao gom phi van chuyen {SHIPPING_FEE})",
                        CreatedDate = order.CreatedAt,
                        IpAddress = "127.0.0.1"
                    };
                    paymentUrl = _vnpay.GetPaymentUrl(paymentRequest);
                    _logger.LogInformation("VNPay URL created for Order {OrderId}", order.Id);
                }
                else if (order.PaymentMethod == PaymentMethod.PayOS && order.PaymentTransactionId.HasValue)
                {
                    try
                    {
                        _logger.LogInformation("Attempting to create PayOS payment link for Order {OrderId}", order.Id);
                        var paymentData = new PaymentData(
                            orderCode: (long)order.PaymentTransactionId,
                            amount: (int)order.TotalAmount, // Includes shipping fee
                            description: $"DH {order.PaymentTransactionId} (bao gom phi van chuyen {SHIPPING_FEE})",
                            items: payOSItems,
                            cancelUrl: _configuration["PayOS:CancelUrl"],
                            returnUrl: _configuration["PayOS:ReturnUrl"]
                        );

                        _logger.LogInformation("PayOS PaymentData being sent: {PaymentData}", JsonSerializer.Serialize(paymentData));
                        CreatePaymentResult createPaymentResult = await _payOS.createPaymentLink(paymentData);
                        _logger.LogInformation("PayOS createPaymentLink result: {Result}", JsonSerializer.Serialize(createPaymentResult));

                        paymentUrl = createPaymentResult.checkoutUrl;
                        if (string.IsNullOrEmpty(paymentUrl))
                        {
                            _logger.LogWarning("PayOS returned a result with a null or empty checkoutUrl for Order {OrderId}", order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An exception occurred while creating PayOS payment link for Order {OrderId}", order.Id);
                    }
                }

                await transaction.CommitAsync();

                var createdOrderWithDetails = await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.User)
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                response.Order = _mapper.Map<OrderDto>(createdOrderWithDetails);
                response.PaymentUrl = paymentUrl;

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

        public async Task<string?> CreatePayOSPaymentLinkAsync(Order order)
        {
            var items = order.OrderItems.Select(oi => new ItemData(
                oi.Product.Name,
                oi.Quantity,
                (int)oi.Price
            )).ToList();

            var paymentData = new Net.payOS.Types.PaymentData(
                orderCode: long.Parse(order.Id.ToString("N")),
                amount: (int)order.TotalAmount, // Includes shipping fee
                description: $"Payment for Order {order.Id} (bao gom phi van chuyen {SHIPPING_FEE})",
                items: items,
                cancelUrl: _configuration["PayOS:CancelUrl"],
                returnUrl: _configuration["PayOS:ReturnUrl"]

            );

            var createPaymentResult = await _payOS.createPaymentLink(paymentData);
            return createPaymentResult.checkoutUrl;
        }

        public async Task<bool> ProcessPayOSWebhookAsync(WebhookType webhookBody)
        {
            try
            {
                _logger.LogInformation("--- BEGIN PayOS Webhook Processing ---");
                WebhookData verifiedData = _payOS.verifyPaymentWebhookData(webhookBody);
                _logger.LogInformation("PayOS Webhook verified for orderCode: {OrderCode}, Code: {Code}", verifiedData.orderCode, verifiedData.code);
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.PaymentTransactionId == verifiedData.orderCode);
                if (order == null)
                {
                    _logger.LogWarning("Order with PaymentTransactionId (orderCode) {OrderCode} NOT FOUND in database.", verifiedData.orderCode);
                    return true;
                }
                if (order.Status != OrderStatus.Pending)
                {
                    _logger.LogWarning("Order {OrderId} has already been processed (Status: {Status}). Skipping update.", order.Id, order.Status);
                    return true;
                }
                if (verifiedData.code == "00")
                {
                    order.Status = OrderStatus.Processing;
                    _logger.LogInformation("Successfully updated Order {OrderId} status to Processing.", order.Id);
                }
                else
                {
                    order.Status = OrderStatus.Cancelled;
                    _logger.LogWarning("PayOS payment was not successful (Code: {Code}). Updated Order {OrderId} status to Cancelled.", verifiedData.code, order.Id);
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("--- END PayOS Webhook Processing (Success) ---");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in ProcessPayOSWebhookAsync for orderCode {OrderCode}.", webhookBody.data?.orderCode);
                return false;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
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
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId)
        {
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
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return null;
            if (!isAdmin && order.UserId != userId) return null;
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<Order?> GetOrderEntityByIdAsync(Guid orderId)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}