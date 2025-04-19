using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UltraStrore.Data;
using UltraStrore.Helper;

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
            // Lấy MaNguoiDung từ token (thay thế ND00013)
            string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? "ND00013";

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
                    FinalAmount = d.FinalAmount,
                    Items = d.ChiTietDonHangs.Select(cd => new
                    {
                        Id = cd.MaCtdh,
                        Name = cd.MaCombo != null
                            ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                            : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                        Quantity = cd.SoLuong,
                        Price = cd.Gia,
                        Image = "/placeholder.svg" // Có thể thay bằng logic lấy từ HinhAnh nếu cần
                    }).ToList(),
                    TenNguoiNhan = d.TenNguoiNhan,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    LyDoHuy = d.LyDoHuy,
                    Sdt = d.Sdt
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/user/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdersByUserId(string id)
        {
            if (string.IsNullOrEmpty(id) || id == "undefined")
            {
                return BadRequest(new { message = "ID người dùng không hợp lệ." });
            }

            var ordersQuery = await _context.DonHangs
                .Where(d => d.MaNguoiDung == id)
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .ThenInclude(sp => sp.HinhAnhs)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .ThenInclude(sp => sp.HinhAnhs)
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new
                {
                    MaDonHang = d.MaDonHang,
                    TenNguoiNhan = d.TenNguoiNhan,
                    NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : DateTime.UtcNow.ToString("dd/MM/yyyy"), // Sử dụng ngày hiện tại nếu null
                    TrangThaiDonHang = (int)d.TrangThaiDonHang,
                    TrangThaiThanhToan = (int)d.TrangThaiHang,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    LyDoHuy = d.LyDoHuy,
                    TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien),
                    FinalAmount = d.FinalAmount,
                    SanPhams = d.ChiTietDonHangs.Select(cd => new
                    {
                        MaChiTietDh = cd.MaCtdh,
                        LaCombo = cd.MaCombo != null,
                        TenSanPham = cd.MaCombo != null
                            ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                            : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                        SoLuong = cd.SoLuong,
                        Gia = cd.Gia,
                        ThanhTien = cd.ThanhTien,
                        MaCombo = cd.MaCombo,
                        MaSanPham = cd.MaSanPham,
                        Combo = cd.MaCombo != null && cd.MaComboNavigation != null ? new
                        {
                            TenCombo = cd.MaComboNavigation.TenComBo,
                            GiaCombo = cd.MaComboNavigation.TongGia,
                            SanPhamsTrongCombo = cd.MaComboNavigation.ChiTietComBos.Select(ct => new
                            {
                                TenSanPham = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                                SoLuong = ct.SoLuong,
                                Gia = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.Gia : 0,
                                ThanhTien = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.Gia * ct.SoLuong : 0,
                                MaSanPham = ct.MaSanPham
                            })
                        } : null
                    }).ToList(),
                    ThongTinNguoiDung = new
                    {
                        TenNguoiNhan = d.TenNguoiNhan,
                        DiaChi = d.DiaChi,
                        Sdt = d.Sdt,
                        TenNguoiDat = d.MaNguoiDungNavigation.HoTen
                    },
                    ThongTinDonHang = new
                    {
                        NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : DateTime.UtcNow.ToString("dd/MM/yyyy"),
                        TrangThai = (int)d.TrangThaiDonHang,
                        ThanhToan = (int)d.TrangThaiHang,
                        HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "Thanh toán khi nhận hàng" : "Thanh toán VNPay"
                    }
                })
                .ToListAsync();

            if (ordersQuery == null || !ordersQuery.Any())
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng nào cho người dùng này." });
            }

            var orders = ordersQuery.Select(d => new
            {
                d.MaDonHang,
                d.TenNguoiNhan,
                d.NgayDat,
                d.TrangThaiDonHang,
                d.TrangThaiThanhToan,
                d.HinhThucThanhToan,
                d.LyDoHuy,
                d.TongTien,
                FinalAmount = d.FinalAmount,
                SanPhams = d.SanPhams.Select(cd => new
                {
                    cd.MaChiTietDh,
                    cd.LaCombo,
                    cd.TenSanPham,
                    cd.SoLuong,
                    cd.Gia,
                    cd.ThanhTien,
                    HinhAnh = cd.LaCombo
                        ? _context.ChiTietComBos
                            .Where(ct => ct.MaComBo == cd.MaCombo)
                            .Select(ct => ct.MaSanPhamNavigation.HinhAnhs.FirstOrDefault())
                            .FirstOrDefault()?.Link
                        : _context.HinhAnhs
                            .Where(h => h.MaSanPham == cd.MaSanPham)
                            .FirstOrDefault()?.Link,
                    Combo = cd.Combo != null ? new
                    {
                        cd.Combo.TenCombo,
                        cd.Combo.GiaCombo,
                        SanPhamsTrongCombo = cd.Combo.SanPhamsTrongCombo.Select(ct => new
                        {
                            ct.TenSanPham,
                            ct.SoLuong,
                            ct.Gia,
                            ct.ThanhTien,
                            HinhAnh = _context.HinhAnhs
                                .Where(h => h.MaSanPham == ct.MaSanPham)
                                .FirstOrDefault()?.Link
                        })
                    } : null
                }).ToList(),
                d.ThongTinNguoiDung,
                d.ThongTinDonHang
            }).ToList();

            return Ok(orders);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUserOrders([FromQuery] string query)
        {
            var claims = User.FindAll(ClaimTypes.NameIdentifier).ToList();
            var maNguoiDung = claims.FirstOrDefault(c => c.Value.StartsWith("ND") || c.Value.StartsWith("KH") || c.Value.StartsWith("AD"))?.Value;
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng trong token." });
            }

            if (string.IsNullOrEmpty(query))
            {
                return await GetUserOrders(); // Nếu không có query, trả về tất cả đơn hàng
            }

            var orders = await _context.DonHangs
                .Where(d => d.MaNguoiDung == maNguoiDung &&
                            (d.MaDonHang.ToString().Contains(query) ||
                             (d.TenNguoiNhan != null && d.TenNguoiNhan.Contains(query)) ||
                             (d.Sdt != null && d.Sdt.Contains(query))))
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .ThenInclude(sp => sp.HinhAnhs)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .ThenInclude(sp => sp.HinhAnhs)
                .Select(d => new
                {
                    Id =   d.MaDonHang,
                    Date = d.NgayDat != null ? d.NgayDat.Value.ToString("yyyy-MM-dd") : "",
                    Status = d.TrangThaiDonHang == TrangThaiDonHang.ChuaXacNhan ? "pending" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangXuLy ? "processing" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangGiaoHang ? "shipping" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "completed" : "canceled",
                    Total = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    FinalAmount = d.FinalAmount,
                    Items = d.ChiTietDonHangs.Select(cd => new
                    {
                        Id = cd.MaCtdh,
                        Name = cd.MaCombo != null
                            ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                            : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                        Quantity = cd.SoLuong,
                        Price = cd.Gia,
                        Image = cd.MaCombo != null
                            ? cd.MaComboNavigation != null && cd.MaComboNavigation.ChiTietComBos.Any()
                                ? cd.MaComboNavigation.ChiTietComBos
                                    .Select(ct => ct.MaSanPhamNavigation != null && ct.MaSanPhamNavigation.HinhAnhs.Any()
                                        ? ct.MaSanPhamNavigation.HinhAnhs.FirstOrDefault().Link
                                        : "/placeholder.svg")
                                    .FirstOrDefault() ?? "/placeholder.svg"
                                : "/placeholder.svg"
                            : cd.MaSanPhamNavigation != null && cd.MaSanPhamNavigation.HinhAnhs.Any()
                                ? cd.MaSanPhamNavigation.HinhAnhs.FirstOrDefault().Link
                                : "/placeholder.svg"
                    }).ToList(),
                    TenNguoiNhan = d.TenNguoiNhan,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    LyDoHuy = d.LyDoHuy,
                    Sdt = d.Sdt
                })
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng nào khớp với tiêu chí tìm kiếm." });
            }

            return Ok(orders);
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