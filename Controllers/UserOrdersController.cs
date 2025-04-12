using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;

namespace UltraStrore.Controllers
{
    [Route("api/user/orders")]
    [ApiController]
    [Authorize]
    public class UserOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/user/orders
        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            string maNguoiDung = "ND00013"; // Thay bằng logic lấy MaNguoiDung thực tế (từ token)

            var orders = await _context.DonHangs
                .Where(d => d.MaNguoiDung == maNguoiDung)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .Select(d => new
                {
                    Id = "ORD-" + d.MaDonHang.ToString("D5"),
                    Date = d.NgayDat != null ? d.NgayDat.Value.ToString("yyyy-MM-dd") : "",
                    Status = d.TrangThaiDonHang == TrangThaiDonHang.ChuaXacNhan ? "pending" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangXuLy ? "processing" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangGiaoHang ? "shipping" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "completed" : "canceled",
                    Total = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    Items = d.ChiTietDonHangs.Select(cd => new
                    {
                        Id = cd.MaCtdh,
                        Name = cd.MaCombo != null
                            ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                            : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                        Quantity = cd.SoLuong,
                        Price = cd.Gia,
                        Image = "/placeholder.svg" // Có thể thay bằng logic lấy từ HinhAnh nếu cần
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/user/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            string maNguoiDung = "ND00013"; // Thay bằng logic lấy MaNguoiDung thực tế

            var order = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == maNguoiDung);

            if (order == null)
            {
                return NotFound(new { message = "Đơn hàng không tồn tại hoặc không thuộc về bạn" });
            }

            var orderDetails = new
            {
                Id = "ORD-" + order.MaDonHang.ToString("D5"),
                Date = order.NgayDat != null ? order.NgayDat.Value.ToString("yyyy-MM-dd") : "",
                Status = order.TrangThaiDonHang == TrangThaiDonHang.ChuaXacNhan ? "pending" :
                         order.TrangThaiDonHang == TrangThaiDonHang.DangXuLy ? "processing" :
                         order.TrangThaiDonHang == TrangThaiDonHang.DangGiaoHang ? "shipping" :
                         order.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "completed" : "canceled",
                Total = order.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                Items = order.ChiTietDonHangs.Select(cd => new
                {
                    Id = cd.MaCtdh,
                    Name = cd.MaCombo != null
                        ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                        : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                    Quantity = cd.SoLuong,
                    Price = cd.Gia,
                    Image = "/placeholder.svg"
                }).ToList()
            };

            return Ok(orderDetails);
        }

        // PUT: api/user/orders/cancel/{id}
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string lyDoHuy)
        {
            string maNguoiDung = "ND00013"; // Thay bằng logic lấy MaNguoiDung thực tế

            var order = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.MaNguoiDung == maNguoiDung);

            if (order == null)
            {
                return NotFound(new { message = "Đơn hàng không tồn tại hoặc không thuộc về bạn" });
            }

            if (order.TrangThaiDonHang != TrangThaiDonHang.ChuaXacNhan && order.TrangThaiDonHang != TrangThaiDonHang.DangXuLy)
            {
                return BadRequest(new { message = "Chỉ có thể hủy đơn hàng khi chưa xác nhận hoặc đang xử lý" });
            }

            var user = await _context.NguoiDungs.FindAsync(order.MaNguoiDung);
            if (user.LockoutEndDate != null && user.LockoutEndDate > DateTime.Now)
            {
                return BadRequest(new { message = $"Tài khoản của bạn bị khóa đến {user.LockoutEndDate.Value.ToString("dd/MM/yyyy HH:mm:ss")}" });
            }

            if (string.IsNullOrEmpty(lyDoHuy))
            {
                return BadRequest(new { message = "Lý do hủy không được để trống" });
            }

            order.TrangThaiDonHang = TrangThaiDonHang.DaHuy;
            order.LyDoHuy = lyDoHuy;

            user.CancelConunt = (user.CancelConunt ?? 0) + 1;
            if (user.CancelConunt > 3)
            {
                user.LockoutEndDate = DateTime.Now.AddDays(3);
                user.TrangThai = 1; // Bị khóa
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Hủy đơn thành công" });
        }
    }
}