using Microsoft.AspNetCore.Http;
using MyanvieBE.DTOs.Order; // Quan trọng: using DTO mới
using MyanvieBE.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}