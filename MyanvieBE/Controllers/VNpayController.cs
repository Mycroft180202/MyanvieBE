using Microsoft.AspNetCore.Mvc;
using MyanvieBE.Services;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService; // Added to call ProcessVnpayPaymentAsync

        public VnpayController(IVnpay vnPayservice, IConfiguration configuration, IOrderService orderService)
        {
            _vnpay = vnPayservice;
            _configuration = configuration;
            _orderService = orderService; // Inject OrderService

            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);
        }

        [HttpGet("CreatePaymentUrl")]
        public ActionResult<string> CreatePaymentUrl(double money, string description)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);

                var request = new PaymentRequest
                {
                    PaymentId = DateTime.Now.Ticks,
                    Money = money,
                    Description = description,
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);

                return Created(paymentUrl, paymentUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("IpnAction")]
        public IActionResult IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    if (paymentResult.IsSuccess)
                    {
                        // Process payment in OrderService
                        var isProcessed = _orderService.ProcessVnpayPaymentAsync(Request.Query).GetAwaiter().GetResult();
                        if (!isProcessed)
                        {
                            return BadRequest("Không thể xử lý thanh toán.");
                        }
                        return Ok();
                    }

                    return BadRequest("Thanh toán thất bại");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        [HttpGet("Callback")]
        public async Task<IActionResult> Callback()
        {
            // Lấy URL của trang frontend từ appsettings.json
            var frontendCallbackUrl = _configuration["Vnpay:FrontendCallbackUrl"];
            if (string.IsNullOrEmpty(frontendCallbackUrl))
            {
                // Dùng một giá trị mặc định nếu không được cấu hình
                frontendCallbackUrl = "http://localhost:3000/payment-result";
            }

            if (Request.QueryString.HasValue)
            {
                // Gọi service để cập nhật database. 
                // Bước này đã chạy thành công ở chỗ bạn.
                await _orderService.ProcessVnpayPaymentAsync(Request.Query);

                // **ĐÂY LÀ THAY ĐỔI QUAN TRỌNG**
                // Tạo URL mới bằng cách ghép URL frontend với toàn bộ tham số từ VNPay.
                var redirectUrl = $"{frontendCallbackUrl}{Request.QueryString.Value}";

                // Trả về lệnh Redirect (HTTP 302) để trình duyệt tự động chuyển hướng.
                return Redirect(redirectUrl);
            }

            // Nếu không có thông tin, chuyển hướng về trang lỗi.
            return Redirect($"{frontendCallbackUrl}?vnp_ResponseCode=99");
        }

    }
}