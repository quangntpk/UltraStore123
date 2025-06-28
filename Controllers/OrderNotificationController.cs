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

        public OrderNotificationController(ApplicationDbContext context, IOrderNotificationService orderNotificationService)
        {
            _context = context;
            _orderNotificationService = orderNotificationService;
        }

        [HttpPost("send-order-confirmation/{orderId}")]
        public async Task<IActionResult> SendOrderConfirmation(int orderId)
        {
            var order = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            string email = order.MaNguoiDungNavigation?.Email ?? "trungtrungg1804@gmail.com";
            string statusMessage = "Đơn hàng của bạn đã được đặt thành công và đang chờ xác nhận.";

            await _orderNotificationService.SendOrderStatusNotificationAsync(email, orderId, statusMessage);

            return Ok(new { message = "Thông báo xác nhận đơn hàng đã được gửi." });
        }

    }
}
