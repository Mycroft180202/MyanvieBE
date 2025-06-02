// MyanvieBE/DTOs/Order/OrderDto.cs
using System;
using System.Collections.Generic;
using MyanvieBE.Models; // Để dùng OrderStatus

namespace MyanvieBE.DTOs.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CustomerFullName { get; set; } // Lấy tên người đặt
        public string CustomerEmail { get; set; }    // Lấy email người đặt
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}