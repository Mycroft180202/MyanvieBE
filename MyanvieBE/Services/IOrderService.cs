using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using MyanvieBE.DTOs.Order;
using MyanvieBE.Models;
using Net.payOS.Types; // THÊM DÒNG NÀY

namespace MyanvieBE.Services
{
    public interface IOrderService
    {
        Task<CreateOrderResponseDto?> CreateOrderAsync(CreateOrderDto createOrderDto, Guid userId);
        Task<bool> ProcessVnpayPaymentAsync(IQueryCollection vnpayResponse);
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId);
        Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<bool> ProcessPayOSWebhookAsync(WebhookType webhookBody);
    }
}