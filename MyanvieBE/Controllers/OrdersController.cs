// MyanvieBE/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyanvieBE.DTOs.Order;
using MyanvieBE.Services;
using System;
using System.Security.Claims; // Để lấy UserId từ token
using System.Threading.Tasks;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // <-- Chỉ người dùng đã đăng nhập mới được tạo đơn hàng
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy UserId của người dùng đang đăng nhập từ token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                               User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("CreateOrder: User ID not found or invalid in token.");
                return Unauthorized(new { message = "Token không hợp lệ hoặc không chứa User ID." });
            }

            _logger.LogInformation("Endpoint POST /api/orders called by User ID: {UserId}", userId);

            try
            {
                var createdOrder = await _orderService.CreateOrderAsync(createOrderDto, userId);
                if (createdOrder == null)
                {
                    // Service có thể trả về null nếu user không tìm thấy (dù đã check token)
                    _logger.LogWarning("Order creation failed for User ID {UserId}, service returned null.", userId);
                    return BadRequest(new { message = "Không thể tạo đơn hàng, vui lòng thử lại." });
                }
                // Trả về 201 Created với thông tin đơn hàng vừa tạo
                // Và một URL để client có thể truy cập tài nguyên vừa tạo (chúng ta sẽ làm endpoint GetOrderById sau)
                return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
            }
            catch (KeyNotFoundException knfEx)
            {
                _logger.LogWarning(knfEx, "Error creating order for User ID {UserId} due to product not found.", userId);
                return BadRequest(new { message = knfEx.Message });
            }
            catch (InvalidOperationException ioEx) // Bắt lỗi không đủ hàng
            {
                _logger.LogWarning(ioEx, "Error creating order for User ID {UserId} due to insufficient stock or other invalid operation.", userId);
                return BadRequest(new { message = ioEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating order for User ID {UserId}", userId);
                return StatusCode(500, new { message = "Đã có lỗi không mong muốn xảy ra khi tạo đơn hàng." });
            }
        }

        // TODO: Tạo một endpoint GetOrderById sau
        [HttpGet("{id}")] // Tạm thời để đây cho CreatedAtAction hoạt động
        public IActionResult GetOrderById(Guid id)
        {
            _logger.LogInformation("Placeholder GetOrderById called for {OrderId}", id);
            // Sau này sẽ triển khai logic lấy đơn hàng theo ID
            return Ok(new { message = $"Endpoint to get order {id} will be implemented here." });
        }

        // GET: api/orders (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xem tất cả đơn hàng
        public async Task<IActionResult> GetAllOrders()
        {
            _logger.LogInformation("Admin endpoint GET /api/orders called by User: {User}", User?.Identity?.Name);
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // PUT: api/orders/{id}/status (Admin only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được cập nhật trạng thái
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] AdminUpdateOrderStatusDto statusDto)
        {
            _logger.LogInformation("Admin endpoint PUT /api/orders/{OrderId}/status called by User: {User} to status {NewStatus}",
                id, User?.Identity?.Name, statusDto.Status);

            if (!ModelState.IsValid) // EnumDataType attribute sẽ giúp validate ở đây
            {
                return BadRequest(ModelState);
            }

            var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, statusDto.Status);

            if (updatedOrder == null)
            {
                _logger.LogWarning("Admin: UpdateOrderStatus failed for Order ID {OrderId} or order not found.", id);
                return NotFound(new { message = "Không tìm thấy đơn hàng hoặc không thể cập nhật trạng thái." });
            }
            return Ok(updatedOrder);
        }
    }
}