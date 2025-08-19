/*using Microsoft.AspNetCore.Http;
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

                    // ✅ Gửi thông báo Telegram cho Admin
                    try
                    {
                        await _telegramServices.SendOrderNotificationAsync(response.OrderId.Value);
                        _logger.LogInformation($"✅ Telegram notification sent for COD order {response.OrderId.Value}");
                    }
                    catch (Exception telegramEx)
                    {
                        _logger.LogError(telegramEx, $"❌ Failed to send Telegram notification for order {response.OrderId.Value}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notifications for order {OrderId}", response.OrderId.Value);
                }

                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("vnpay-callback")]
        public async Task VnPayCallback()
        {
            var query = HttpContext.Request.Query;

            _logger.LogInformation("🔍 VNPay callback received. Response Code: {ResponseCode}, TxnRef: {TxnRef}",
                query["vnp_ResponseCode"], query["vnp_TxnRef"]);

            // Xử lý callback VNPay
            await _paymentService.ProcessVnPayCallbackAsync(query, HttpContext);

            // ✅ Gửi Telegram nếu thanh toán VNPay thành công
            if (query.ContainsKey("vnp_ResponseCode") && query["vnp_ResponseCode"] == "00")
            {
                try
                {
                    if (query.ContainsKey("vnp_TxnRef") && int.TryParse(query["vnp_TxnRef"], out int orderId))
                    {
                        _logger.LogInformation("💳 VNPay payment successful for order {OrderId}, sending Telegram...", orderId);

                        await _telegramServices.SendOrderNotificationAsync(orderId);
                        _logger.LogInformation($"✅ Telegram notification sent for VNPay order {orderId}");
                    }
                    else
                    {
                        _logger.LogWarning("❌ Cannot parse vnp_TxnRef: {TxnRef}", query["vnp_TxnRef"]);
                    }
                }
                catch (Exception telegramEx)
                {
                    _logger.LogError(telegramEx, "❌ Failed to send Telegram notification for VNPay order");
                }
            }
            else
            {
                _logger.LogWarning("💳 VNPay payment failed or pending. Response code: {ResponseCode}", query["vnp_ResponseCode"]);
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

                    // ✅ Gửi thông báo Telegram cho Admin (Instant Checkout)
                    try
                    {
                        await _telegramServices.SendOrderNotificationAsync(response.OrderId.Value);
                        _logger.LogInformation($"✅ Telegram notification sent for instant checkout order {response.OrderId.Value}");
                    }
                    catch (Exception telegramEx)
                    {
                        _logger.LogError(telegramEx, $"❌ Failed to send Telegram notification for instant checkout order {response.OrderId.Value}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notifications for instant checkout order {OrderId}", response.OrderId.Value);
                }

                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}