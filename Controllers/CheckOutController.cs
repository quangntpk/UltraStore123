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
        private readonly ITelegramServices _telegramServices;
        private readonly ILogger<CheckOutController> _logger;
        private readonly ApplicationDbContext _context;

        public CheckOutController(
            ICheckOutServices paymentService,
            IOrderNotificationService orderNotificationService,
            ITelegramServices telegramServices,
            ILogger<CheckOutController> logger,
            ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _orderNotificationService = orderNotificationService;
            _telegramServices = telegramServices;
            _logger = logger;
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
                // ✅ GỬI EMAIL CHO TẤT CẢ PHƯƠNG THỨC THANH TOÁN (COD VÀ CASH)
                await SendOrderConfirmationEmail(response.OrderId.Value, request.PaymentMethod);
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("vnpay-callback")]
        public async Task VnPayCallback()
        {
            var query = HttpContext.Request.Query;

            // ✅ THÊM: Lấy callback response để biết OrderId
            var callbackResponse = await _paymentService.ProcessVnPayCallbackAsync(query, HttpContext);

            // ✅ GỬI EMAIL SAU KHI VNPAY CALLBACK THÀNH CÔNG
            if (callbackResponse != null && callbackResponse.Success && callbackResponse.OrderId.HasValue)
            {
                await SendOrderConfirmationEmail(callbackResponse.OrderId.Value, "VNPay");
            }
        }

        // ✅ HELPER METHOD: Gửi email xác nhận đơn hàng
        private async Task SendOrderConfirmationEmail(int orderId, string paymentMethod)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Sending confirmation email for order {orderId}, payment method: {paymentMethod}");

                var order = await _context.DonHangs
                    .Include(d => d.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

                if (order != null && order.MaNguoiDungNavigation != null && !string.IsNullOrEmpty(order.MaNguoiDungNavigation.Email))
                {
                    string email = order.MaNguoiDungNavigation.Email;
                    string statusMessage = paymentMethod?.ToLower() == "vnpay"
                        ? "Đơn hàng của bạn đã được thanh toán thành công qua VNPay và đang được xử lý."
                        : paymentMethod?.ToLower() == "cash"
                        ? "Đơn hàng của bạn đã được thanh toán bằng tiền mặt thành công."
                        : "Đơn hàng của bạn đã được đặt thành công và đang chờ xác nhận.";

                    await _orderNotificationService.SendOrderStatusNotificationAsync(
                        email,
                        order.MaDonHang,
                        statusMessage);

                    Console.WriteLine($"[SUCCESS] Email sent successfully to {email} for order {orderId}");
                    try
                    {
                        await _telegramServices.SendOrderNotificationAsync(orderId);
                        _logger.LogInformation($"✅ Telegram notification sent for COD order {orderId}");
                    }
                    catch (Exception telegramEx)
                    {
                        _logger.LogError(telegramEx, $"❌ Failed to send Telegram notification for order {orderId}");
                    }
                }
                else
                {
                    Console.WriteLine($"[WARNING] Cannot send email - Order {orderId} not found or missing email");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send email for order {orderId}: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            }
        }
        [HttpPost("test-email/{orderId}")]
        public async Task<IActionResult> TestEmail(int orderId, [FromQuery] string paymentMethod = "COD")
        {
            try
            {
                await SendOrderConfirmationEmail(orderId, paymentMethod);
                return Ok(new { message = "Email sent successfully", orderId, paymentMethod });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to send email", error = ex.Message });
            }
        }
        [HttpPost("InstantCheckout")]
        public async Task<IActionResult> InstantCheckout([FromBody] PaymentRequestDto1 request)
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

            var response = await _paymentService.InstantCheckout(request, HttpContext);

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
    }
}