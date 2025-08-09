using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderNotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderNotificationService _orderNotificationService;
        private readonly IQRCodeService _qrCodeService;

        public OrderNotificationController(
            ApplicationDbContext context,
            IOrderNotificationService orderNotificationService,
            IQRCodeService qrCodeService)
        {
            _context = context;
            _orderNotificationService = orderNotificationService;
            _qrCodeService = qrCodeService;
        }

        [HttpPost("send-order-confirmation/{orderId}")]
        public async Task<IActionResult> SendOrderConfirmation(int orderId)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Attempting to send confirmation for order {orderId}");

                var order = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaSanPhamNavigation)
                    .Include(d => d.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

                if (order == null)
                {
                    Console.WriteLine($"[ERROR] Order {orderId} not found");
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                string email = order.MaNguoiDungNavigation?.Email ?? "trungtrungg1804@gmail.com";
                string statusMessage = "Đơn hàng của bạn đã được đặt thành công và đang chờ xác nhận.";

                Console.WriteLine($"[DEBUG] Sending email to: {email}");

                // ✅ FIX: Sử dụng method rõ ràng
                await _orderNotificationService.SendOrderStatusNotificationAsync(email, orderId, statusMessage);

                return Ok(new
                {
                    message = "Thông báo xác nhận đơn hàng đã được gửi.",
                    orderId = orderId,
                    email = email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send order confirmation: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi gửi email xác nhận.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("test-qr/{orderId}")]
        public async Task<IActionResult> TestQRCode(int orderId)
        {
            try
            {
                var order = await _context.DonHangs.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                string qrLink = $"http://localhost:8080/user/hoadon?orderId={order.MaDonHang}";
                var qrBytes = _qrCodeService.GenerateQRCode(qrLink, 10);
                return File(qrBytes, "image/png", $"test-qr-{orderId}.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Test QR failed: {ex.Message}");

                // ✅ FIX: Fallback sử dụng method alternative
                try
                {
                    string qrLink = $"http://localhost:8080/user/hoadon?orderId={orderId}";
                    var qrBytes = _qrCodeService.GenerateQRCodeAlternative(qrLink, 10);
                    return File(qrBytes, "image/png", $"test-qr-{orderId}.png");
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"[ERROR] Fallback QR failed: {fallbackEx.Message}");
                    return StatusCode(500, new { message = $"QR generation failed: {ex.Message}" });
                }
            }
        }

        [HttpGet("test-qr-base64/{orderId}")]
        public async Task<IActionResult> TestQRCodeBase64(int orderId)
        {
            try
            {
                var order = await _context.DonHangs.FindAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                string qrLink = $"http://localhost:8080/user/hoadon?orderId={order.MaDonHang}";
                var qrBase64 = _qrCodeService.GenerateQRCodeBase64(qrLink, 10);

                return Ok(new
                {
                    orderId = orderId,
                    qrCode = qrBase64,
                    dataUrl = $"data:image/png;base64,{qrBase64}",
                    link = qrLink
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Test QR Base64 failed: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}