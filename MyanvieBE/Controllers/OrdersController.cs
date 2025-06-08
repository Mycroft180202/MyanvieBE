// MyanvieBE/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyanvieBE.DTOs.Order;
using MyanvieBE.Models;
using MyanvieBE.Services;
using System.Security.Claims;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        private bool TryGetUserIdAndRole(out Guid userId, out bool isAdmin)
        {
            userId = Guid.Empty;
            isAdmin = User.IsInRole("Admin");
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out userId))
            {
                return false;
            }
            return true;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetUserIdAndRole(out var userId, out _))
                return Unauthorized();

            try
            {
                var createdOrder = await _orderService.CreateOrderAsync(createOrderDto, userId);
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex) when (ex is KeyNotFoundException || ex is InvalidOperationException)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating order for User ID {UserId}", userId);
                return StatusCode(500, new { message = "Đã có lỗi không mong muốn xảy ra." });
            }
        }

        // GET: api/orders/my-orders
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            if (!TryGetUserIdAndRole(out var userId, out _))
                return Unauthorized();

            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            if (!TryGetUserIdAndRole(out var userId, out var isAdmin))
                return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(id, userId, isAdmin);
            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập." });
            }
            return Ok(order);
        }

        // GET: api/orders (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // PUT: api/orders/{id}/status (Admin only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] AdminUpdateOrderStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, statusDto.Status);
            if (updatedOrder == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }
            return Ok(updatedOrder);
        }
    }
}