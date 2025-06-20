using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyanvieBE.Services;
using Net.payOS.Types; // Thêm using này
using System.Threading.Tasks;

namespace MyanvieBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IOrderService orderService, IConfiguration configuration, ILogger<WebhookController> logger)
        {
            _orderService = orderService;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: /api/webhook/vnpay-callback
        [HttpGet("vnpay-callback")]
        public async Task<IActionResult> VnpayCallback()
        {
            var frontendCallbackUrl = _configuration["Vnpay:FrontendCallbackUrl"];
            if (string.IsNullOrEmpty(frontendCallbackUrl))
            {
                frontendCallbackUrl = "http://localhost:3000/payment-result";
            }

            if (Request.QueryString.HasValue)
            {
                await _orderService.ProcessVnpayPaymentAsync(Request.Query);
                var redirectUrl = $"{frontendCallbackUrl}{Request.QueryString.Value}";
                return Redirect(redirectUrl);
            }

            return Redirect($"{frontendCallbackUrl}?vnp_ResponseCode=99");
        }

        // POST: /api/webhook/payos
        [HttpPost("payos")]
        public async Task<IActionResult> PayOSWebhook([FromBody] WebhookType webhookBody)
        {
            _logger.LogInformation("Received PayOS webhook request.");
            var result = await _orderService.ProcessPayOSWebhookAsync(webhookBody);
            if (!result)
            {
                _logger.LogError("PayOS webhook processing failed. Returning BadRequest.");
                return BadRequest("Webhook processing failed.");
            }
            return Ok();
        }
    }
}