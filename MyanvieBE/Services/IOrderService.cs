// MyanvieBE/Services/IOrderService.cs
using MyanvieBE.DTOs.Order;
using MyanvieBE.Models;
using System;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public interface IOrderService
    {
        Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto, Guid userId);
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId);
        Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync(); // <-- THÊM DÒNG NÀY (Admin)
        Task<OrderDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus); // <-- THÊM DÒNG NÀY (Admin)
    }
}