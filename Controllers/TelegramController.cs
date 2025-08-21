using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Repository;
using UltraStrore.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TelegramController : ControllerBase
    {
        private readonly ITelegramServices _telegramServices;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TelegramController> _logger;

        public TelegramController(
            ITelegramServices telegramServices,
            ApplicationDbContext context,
            ILogger<TelegramController> logger)
        {
            _telegramServices = telegramServices;
            _context = context;
            _logger = logger;
        }

        private async Task<bool> IsAdmin()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            User.FindFirst("maNguoiDung")?.Value ??
                            User.FindFirst("nameid")?.Value ??
                            User.FindFirst("sub")?.Value ??
                            User.FindFirst("userId")?.Value ??
                            User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(userId)) return false;

                var user = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.MaNguoiDung == userId || u.Email == userId);

                return user?.VaiTro == 1; // Chỉ Admin
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // ✅ SỬA: Dùng StatusCode thay vì Forbid()
                if (!await IsAdmin())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Chỉ Admin mới có quyền test Telegram Bot"
                    });
                }

                await _telegramServices.TestConnectionAsync();
                return Ok(new
                {
                    success = true,
                    message = "Test message sent successfully!",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test Telegram connection");
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to send test message",
                    error = ex.Message
                });
            }
        }

        [HttpPost("notify-order/{orderId}")]
        public async Task<IActionResult> NotifyOrder(int orderId)
        {
            try
            {
                // ✅ SỬA: Dùng StatusCode thay vì Forbid()
                if (!await IsAdmin())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Chỉ Admin mới có quyền gửi thông báo Telegram"
                    });
                }

                var orderExists = await _context.DonHangs
                    .AnyAsync(d => d.MaDonHang == orderId);

                if (!orderExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Đơn hàng {orderId} không tồn tại"
                    });
                }

                await _telegramServices.SendOrderNotificationAsync(orderId);

                return Ok(new
                {
                    success = true,
                    message = $"Đã gửi thông báo cho đơn hàng #{orderId}",
                    orderId = orderId,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order notification for order {OrderId}", orderId);
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể gửi thông báo",
                    error = ex.Message
                });
            }
        }

        [HttpPost("send-custom")]
        public async Task<IActionResult> SendCustomMessage([FromBody] CustomMessageRequest request)
        {
            try
            {
                // ✅ SỬA: Dùng StatusCode thay vì Forbid()
                if (!await IsAdmin())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Chỉ Admin mới có quyền gửi tin nhắn tùy chỉnh"
                    });
                }

                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Nội dung tin nhắn không được để trống"
                    });
                }

                await _telegramServices.SendNewOrderNotificationAsync(new
                {
                    MaDonHang = "CUSTOM",
                    TenNguoiNhan = request.Title ?? "Thông báo tùy chỉnh",
                    Sdt = "",
                    DiaChi = "",
                    ChiTietDonHangs = new List<object>(),
                    FinalAmount = 0,
                    TrangThaiHang = 0,
                    CustomMessage = request.Message
                });

                return Ok(new
                {
                    success = true,
                    message = "Đã gửi tin nhắn tùy chỉnh",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send custom message");
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể gửi tin nhắn",
                    error = ex.Message
                });
            }
        }

        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10)
        {
            try
            {
                // ✅ SỬA: Dùng StatusCode thay vì Forbid()
                if (!await IsAdmin())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Chỉ Admin mới có quyền xem danh sách đơn hàng"
                    });
                }

                var recentOrders = await _context.DonHangs
                    .Include(d => d.MaNguoiDungNavigation)
                    .OrderByDescending(d => d.NgayDat)
                    .Take(limit)
                    .Select(d => new
                    {
                        MaDonHang = d.MaDonHang,
                        TenNguoiNhan = d.TenNguoiNhan,
                        NgayDat = d.NgayDat,
                        TrangThaiDonHang = (int)d.TrangThaiDonHang,
                        TrangThaiThanhToan = (int)d.TrangThaiHang,
                        FinalAmount = d.FinalAmount,
                        KhachHang = d.MaNguoiDungNavigation.HoTen
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = recentOrders,
                    total = recentOrders.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent orders");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server",
                    error = ex.Message
                });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetBotStatus()
        {
            try
            {
                // ✅ SỬA: Dùng StatusCode thay vì Forbid()
                if (!await IsAdmin())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Chỉ Admin mới có quyền kiểm tra trạng thái bot"
                    });
                }

                await _telegramServices.TestConnectionAsync();

                return Ok(new
                {
                    success = true,
                    status = "Bot đang hoạt động",
                    lastCheck = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    status = "Bot không hoạt động",
                    error = ex.Message,
                    lastCheck = DateTime.Now
                });
            }
        }

        [HttpGet("allowed-chats")]
        public async Task<IActionResult> GetAllowedChats()
        {
            try
            {
                // ✅ SỬA: Dùng StatusCode thay vì Forbid()
                if (!await IsAdmin())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        message = "Chỉ Admin mới có quyền xem danh sách người nhận"
                    });
                }

                var allowedChats = _telegramServices.GetAllowedChatIds();

                return Ok(new
                {
                    success = true,
                    totalRecipients = allowedChats.Count,
                    chatIds = allowedChats,
                    message = $"Có {allowedChats.Count} người được phép nhận thông báo"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get allowed chat IDs");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server",
                    error = ex.Message
                });
            }
        }
    }

    public class CustomMessageRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
}