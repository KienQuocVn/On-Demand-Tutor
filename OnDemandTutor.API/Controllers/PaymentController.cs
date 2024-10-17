using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using OnDemandTutor.Contract.Services.Interface;
using OnDemandTutor.ModelViews.AuthModelViews;

namespace OnDemandTutor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly IConfiguration _configuration;

        public PaymentController(IVNPayService vnPayService, IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _configuration = configuration;
        }

        [HttpGet("")]
        public IActionResult Home()
        {
            return Ok("createOrder");
        }

        [HttpPost("submitOrder")]
        public IActionResult SubmitOrder([FromQuery] string amount)
        {
            try
            {
                var paymentInfo = new PaymentInfo
                {
                    OrderId = 112,
                    FullName = "Nguyen Van A",
                    Description = "",
                    Amount = decimal.Parse(amount),
                    CreatedDate = DateTime.UtcNow.AddHours(7)
                };
                var Url = _vnPayService.CreatePaymentUrl(HttpContext, paymentInfo);
                return Ok(Url);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("vnpay-payment-return")]
        public IActionResult PaymentCompleted()
        {
            var response = _vnPayService.PaymentExecute(HttpContext.Request.Query);
            if (response == null || response?.VnPayResponseCode != "00")
            {
                return StatusCode(500, new { message = $"Lỗi thanh toán VNPay: {response?.VnPayResponseCode ?? "unknown error"}" });
            }
            return Ok(response);
        }
    }
}
