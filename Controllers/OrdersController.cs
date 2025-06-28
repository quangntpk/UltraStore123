using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using System.Security.Claims;


namespace UltraStrore.Controllers
{
    //[Authorize(Roles = "1")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        //haha
        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaComboNavigation)
                .Select(d => new
                {
                    MaDonHang = d.MaDonHang,
                    TenNguoiNhan = d.TenNguoiNhan,
                    NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : "",
                    TrangThaiDonHang = (int)d.TrangThaiDonHang,
                    TrangThaiThanhToan = (int)d.TrangThaiHang,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    LyDoHuy = d.LyDoHuy,
                    TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    FinalAmount = d.FinalAmount,

                    // Lấy tên sản phẩm hoặc combo đầu tiên
                    TenSanPhamHoacCombo = d.ChiTietDonHangs.Select(cd => cd.MaCombo != null
                        ? (cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại")
                        : (cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại"))
                        .FirstOrDefault(),

                    // Đây là mã người duyệt đơn (nếu có)
                    MaNguoiDung = d.MaNguoiDung,
                    HoTenNguoiDuyet = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.HoTen : null
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(orders);
        }



        // GET: api/orders/{id}
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
                    TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    FinalAmount = d.FinalAmount ?? 0,
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

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var order = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (order == null)
                return NotFound(new { message = "Đơn hàng không tồn tại" });

            // Phân quyền: nếu không phải admin thì kiểm tra nhân viên được phép xử lý
            if (userRole != "1")
            {
                if (!string.IsNullOrEmpty(order.MaNhanVien))
                {
                    if (order.MaNhanVien != userId)
                        return Forbid("Đơn hàng đã được xử lý bởi nhân viên khác.");
                }
                else
                {
                    order.MaNhanVien = userId;
                }
            }

            // Chỉ duyệt nếu trạng thái hợp lệ
            if (order.TrangThaiDonHang != TrangThaiDonHang.ChuaXacNhan &&
                order.TrangThaiDonHang != TrangThaiDonHang.DangXuLy &&
                order.TrangThaiDonHang != TrangThaiDonHang.DangGiaoHang)
            {
                return BadRequest(new { message = "Không thể duyệt đơn hàng ở trạng thái này" });
            }

            // Cập nhật trạng thái đơn hàng (tăng lên một cấp)
            order.TrangThaiDonHang = (TrangThaiDonHang)((int)order.TrangThaiDonHang + 1);

            // Nếu đã giao hàng thì đánh dấu là đã thanh toán
            if (order.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang)
            {
                order.TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay;
            }

            _context.DonHangs.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Duyệt đơn thành công" });
        }



        // PUT: api/orders/cancel/{id}
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string lyDoHuy)
        {
            var order = await _context.DonHangs.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Đơn hàng không tồn tại" });
            }

            if (order.TrangThaiDonHang != TrangThaiDonHang.ChuaXacNhan && order.TrangThaiDonHang != TrangThaiDonHang.DangXuLy)
            {
                return BadRequest(new { message = "Chỉ có thể hủy đơn hàng khi chưa xác nhận hoặc đang xử lý" });
            }

            if (string.IsNullOrEmpty(lyDoHuy))
            {
                return BadRequest(new { message = "Lý do hủy không được để trống" });
            }

            order.TrangThaiDonHang = TrangThaiDonHang.DaHuy;
            order.LyDoHuy = lyDoHuy;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Hủy đơn thành công" });
        }
    }
}