/*using Microsoft.AspNetCore.Http;
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
*/





using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Helper;
using UltraStrore.Models.DTO;
using UltraStrore.Repository;
using UltraStrore.Data;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckOutController : ControllerBase
    {
        private readonly ICheckOutServices _paymentService;
        private readonly IOrderNotificationService _orderNotificationService;
        private readonly ApplicationDbContext _context;

        public CheckOutController(
            ICheckOutServices paymentService,
            IOrderNotificationService orderNotificationService,
            ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _orderNotificationService = orderNotificationService;
            _context = context;
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

            if (response.Success && response.OrderId.HasValue)
            {
                try
                {
                    var order = await _context.DonHangs
                        .Include(d => d.MaNguoiDungNavigation)
                        .FirstOrDefaultAsync(d => d.MaDonHang == response.OrderId.Value);

                    if (order != null && order.MaNguoiDungNavigation != null && !string.IsNullOrEmpty(order.MaNguoiDungNavigation.Email))
                    {
                        string email = order.MaNguoiDungNavigation.Email;
                        string statusMessage = "Đơn hàng của bạn đã được đặt thành công và đang chờ xác nhận.";

                        await _orderNotificationService.SendOrderStatusNotificationAsync(
                            email,
                            order.MaDonHang,
                            statusMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LỖI EMAIL] Không thể gửi email xác nhận đơn hàng: {ex.Message}");
                }

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
