// MyanvieBE/DTOs/Order/AdminUpdateOrderStatusDto.cs
using MyanvieBE.Models; // Để dùng OrderStatus enum
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Order
{
    public class AdminUpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc.")]
        // EnumDataType sẽ giúp Swagger hiển thị các lựa chọn cho OrderStatus
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Giá trị trạng thái không hợp lệ.")]
        public OrderStatus Status { get; set; }
    }
}