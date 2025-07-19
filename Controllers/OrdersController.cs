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
                .Include(d => d.MaNhanVienNavigation)
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

                    // Thông tin nhân viên duyệt đơn
                    MaNhanVien = d.MaNhanVien,
                    HoTenNhanVien = d.MaNhanVienNavigation != null ? d.MaNhanVienNavigation.HoTen : null,

                    // Thông tin khách hàng đặt hàng
                    MaNguoiDung = d.MaNguoiDung,
                    HoTenKhachHang = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.HoTen : null
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
                .Where(d => d.MaDonHang == int.Parse(id))
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
                                TenSanPham = _context.SanPhams.Where(g => g.MaSanPham == ct.MaSanPham).Select(g => g.TenSanPham).FirstOrDefault(),
                                SoLuong = ct.SoLuong,
                                Gia = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.Gia : 0,
                                ThanhTien = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.Gia * ct.SoLuong : 0,
                                MaSanPham = ct.MaSanPham,
                                MaSanPham1 = _context.DonHangSupports.Where(g => g.MaChiTietCombo == ct.MaChiTietComBo && g.ChiTietGioHang == cd.MaCtdh).Select(g => g.MaSanPham).FirstOrDefault()
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
            var temp = ordersQuery;

            return Ok(temp);
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveOrder(int id, [FromBody] ApproveOrderRequest request)
        {
            // Nếu có Authorization, lấy từ token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không có Authorization, lấy từ request body
            if (string.IsNullOrEmpty(userId))
            {
                userId = request.UserId;
            }

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Không thể xác định người duyệt đơn" });
            }

            // **SỬA: Kiểm tra userId có tồn tại trong bảng NGUOI_DUNG không**
            // Thêm điều kiện kiểm tra cả MaNguoiDung và các trường khác
            var userExists = await _context.NguoiDungs.AnyAsync(u =>
                u.MaNguoiDung == userId ||
                u.Email == userId ||
                u.TaiKhoan == userId);

            if (!userExists)
            {
                return BadRequest(new { message = $"Người dùng {userId} không tồn tại trong hệ thống" });
            }

            // **SỬA: Lấy MaNguoiDung thực sự từ database**
            var actualUserId = await _context.NguoiDungs
                .Where(u => u.MaNguoiDung == userId || u.Email == userId || u.TaiKhoan == userId)
                .Select(u => u.MaNguoiDung)
                .FirstOrDefaultAsync();

            var order = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (order == null)
                return NotFound(new { message = "Đơn hàng không tồn tại" });

            // Kiểm tra trạng thái hợp lệ để duyệt
            if (order.TrangThaiDonHang != Data.TrangThaiDonHang.ChuaXacNhan &&
                order.TrangThaiDonHang != Data.TrangThaiDonHang.DangXuLy &&
                order.TrangThaiDonHang != Data.TrangThaiDonHang.DangGiaoHang)

            {
                return BadRequest(new { message = "Không thể duyệt đơn hàng ở trạng thái này" });
            }

            // **SỬA: Sử dụng actualUserId thay vì userId**
            // Nếu đơn hàng chưa có nhân viên xử lý (bước 1: chưa xác nhận)
            if (string.IsNullOrEmpty(order.MaNhanVien))
            {
                // Gán nhân viên hiện tại làm người xử lý
                order.MaNhanVien = actualUserId;
            }
            else
            {
                // Kiểm tra xem có phải nhân viên được gán xử lý không
                if (order.MaNhanVien != actualUserId)
                {
                    return BadRequest(new { message = "Đơn hàng đã được gán cho nhân viên khác xử lý." });
                }
            }

            // Cập nhật trạng thái đơn hàng (tăng lên một cấp)
            order.TrangThaiDonHang = (Data.TrangThaiDonHang)((int)order.TrangThaiDonHang + 1);

            // Nếu đã giao hàng thì đánh dấu là đã thanh toán
            if (order.TrangThaiDonHang == Data.TrangThaiDonHang.DaGiaoHang)
            {
                order.TrangThaiHang = Data.TrangThaiThanhToan.ThanhToanVNPay;
            }

            try
            {
                _context.DonHangs.Update(order);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Duyệt đơn thành công", assignedStaff = actualUserId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi lưu dữ liệu: {ex.Message}" });
            }
        }

        // Thêm class request
        public class ApproveOrderRequest
        {
            public string UserId { get; set; }
        }



        public enum TrangThaiDonHang
        {
            ChuaXacNhan = 0,
            DangXuLy = 1,
            DangGiaoHang = 2,
            DaGiaoHang = 3,
            DaHuy = 4  // **ĐẢM BẢO giá trị này = 4**
        }

        // **KIỂM TRA: Trong method CancelOrder, đảm bảo SaveChanges được gọi**
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string lyDoHuy)
        {
            var order = await _context.DonHangs.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Đơn hàng không tồn tại" });
            }

            if (order.TrangThaiDonHang != Data.TrangThaiDonHang.ChuaXacNhan &&
                 order.TrangThaiDonHang != Data.TrangThaiDonHang.DangXuLy)
            {
                return BadRequest(new { message = "Chỉ có thể hủy đơn hàng khi chưa xác nhận hoặc đang xử lý" });
            }

            if (string.IsNullOrEmpty(lyDoHuy))
            {
                return BadRequest(new { message = "Lý do hủy không được để trống" });
            }

            order.TrangThaiDonHang = Data.TrangThaiDonHang.DaHuy;
            order.LyDoHuy = lyDoHuy;

            try
            {
                _context.DonHangs.Update(order); // **THÊM dòng này nếu chưa có**
                await _context.SaveChangesAsync();
                return Ok(new { message = "Hủy đơn thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi hủy đơn: {ex.Message}" });
            }
        }
    }
}