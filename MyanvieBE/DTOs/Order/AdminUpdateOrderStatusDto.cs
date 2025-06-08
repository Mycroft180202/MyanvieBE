// MyanvieBE/DTOs/Order/AdminUpdateOrderStatusDto.cs
using MyanvieBE.Models;
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class AdminUpdateOrderStatusDto
    {
        [Required]
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus Status { get; set; }
    }
}