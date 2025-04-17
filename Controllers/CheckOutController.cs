using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Helper;
using UltraStrore.Models.DTO;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckOutController : ControllerBase
    {
        private readonly ICheckOutServices _paymentService;
        public CheckOutController(ICheckOutServices paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessCODPayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Dữ liệu đầu vào không hợp lệ",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var response = await _paymentService.ProcessPaymentAsync(request, HttpContext);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("vnpay-callback")]
        public async Task VnPayCallback()
        {
            var query = HttpContext.Request.Query;
            await _paymentService.ProcessVnPayCallbackAsync(query, HttpContext);
        }
    }
}
