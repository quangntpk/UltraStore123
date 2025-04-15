using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
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
                    TenSanPhamHoacCombo = d.ChiTietDonHangs.Select(cd => cd.MaCombo != null
                        ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                        : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại")
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            try
            {
                var order = await _context.DonHangs
               .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

                if (order == null)
                {
                    return NotFound();
                }

                var orderDetails = new
                {
                    SanPhams = order.ChiTietDonHangs.Select(cd => new
                    {
                        MaChiTietDh = cd.MaCtdh,
                        LaCombo = cd.MaCombo != null,
                        TenSanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(cd.MaSanPham)).Select(g => g.TenSanPham).FirstOrDefault(),
                        SoLuong = cd.SoLuong,
                        Gia = cd.Gia,
                        ThanhTien = cd.ThanhTien,
                        Combo = cd.MaCombo != null && cd.MaComboNavigation != null ? new
                        {
                            TenCombo = cd.MaComboNavigation.TenComBo,
                            GiaCombo = cd.MaComboNavigation.TongGia,
                            SanPhamsTrongCombo = cd.MaComboNavigation.ChiTietComBos.Select(ct => new
                            {
                                TenSanPham = _context.SanPhams.Where(g => g.MaSanPham.Contains(ct.MaSanPham)).Select(g => g.TenSanPham).FirstOrDefault(),
                                SoLuong = ct.SoLuong,
                                Gia = _context.SanPhams.Where(g => g.MaSanPham.Contains(ct.MaSanPham)).Select(g => g.Gia).FirstOrDefault(),
                                ThanhTien = _context.SanPhams.Where(g => g.MaSanPham.Contains(ct.MaSanPham)).Select(g => g.Gia).FirstOrDefault() * ct.SoLuong
                            })
                        } : null
                    }),
                    ThongTinNguoiDung = new
                    {
                        TenNguoiNhan = order.TenNguoiNhan,
                        DiaChi = order.DiaChi,
                        Sdt = order.Sdt,
                        TenNguoiDat = order.MaNguoiDungNavigation.HoTen
                    },
                    ThongTinDonHang = new
                    {
                        NgayDat = order.NgayDat != null ? order.NgayDat.Value.ToString("dd/MM/yyyy") : "",
                        TrangThai = (int)order.TrangThaiDonHang,
                        ThanhToan = (int)order.TrangThaiHang,
                        HinhThucThanhToan = order.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "Thanh toán khi nhận hàng" : "Thanh toán VNPay"
                    }
                };
                var item = orderDetails;
                int i = -1;
                return Ok(orderDetails);
            }
            catch(Exception ex)
            {
                var item = ex.Message;
                int i = -1;
                return BadRequest(ex.Message);
            }
 
        }

        // PUT: api/orders/approve/{id}
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var order = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (order == null)
            {
                return NotFound(new { message = "Đơn hàng không tồn tại" });
            }

            // Kiểm tra trạng thái đơn hàng
            if (order.TrangThaiDonHang != TrangThaiDonHang.ChuaXacNhan &&
                order.TrangThaiDonHang != TrangThaiDonHang.DangXuLy &&
                order.TrangThaiDonHang != TrangThaiDonHang.DangGiaoHang)
            {
                return BadRequest(new { message = "Không thể duyệt đơn hàng ở trạng thái này" });
            }

            // Kiểm tra số lượng tồn kho
            foreach (var chiTiet in order.ChiTietDonHangs)
            {
                if (chiTiet.MaCombo != null)
                {
                    // Nếu là combo
                    var combo = chiTiet.MaComboNavigation;
                    if (combo == null)
                    {
                        return BadRequest(new { message = $"Combo {chiTiet.MaCombo} không tồn tại" });
                    }

                    // Kiểm tra số lượng tồn kho của combo
                    if (combo.SoLuong < chiTiet.SoLuong)
                    {
                        return BadRequest(new { message = $"Số lượng tồn kho của combo {combo.TenComBo} không đủ (còn {combo.SoLuong})" });
                    }

                    // Kiểm tra số lượng tồn kho của các sản phẩm trong combo
                    foreach (var chiTietCombo in combo.ChiTietComBos)
                    {
                        // Lấy mã sản phẩm cơ bản (ví dụ: Q00001)
                        var baseProductCode = chiTietCombo.MaSanPham;

                        // Tìm sản phẩm trong bảng SanPham có mã bắt đầu bằng baseProductCode
                        var sanPham = await _context.SanPhams
                            .FirstOrDefaultAsync(p => p.MaSanPham.StartsWith(baseProductCode));

                        if (sanPham == null)
                        {
                            return BadRequest(new { message = $"Sản phẩm {baseProductCode} trong combo không tồn tại" });
                        }

                        int soLuongCan = (chiTietCombo.SoLuong ?? 0) * (chiTiet.SoLuong ?? 0); // Số lượng cần cho mỗi sản phẩm trong combo
                        if (sanPham.SoLuong < soLuongCan)
                        {
                            return BadRequest(new { message = $"Số lượng tồn kho của sản phẩm {sanPham.TenSanPham} trong combo không đủ (còn {sanPham.SoLuong})" });
                        }
                    }
                }
                else
                {
                    // Nếu là sản phẩm đơn lẻ
                    var sanPham = await _context.SanPhams
                        .FirstOrDefaultAsync(p => p.MaSanPham == chiTiet.MaSanPham);

                    if (sanPham == null)
                    {
                        return BadRequest(new { message = $"Sản phẩm {chiTiet.MaSanPham} không tồn tại" });
                    }

                    if (sanPham.SoLuong < chiTiet.SoLuong)
                    {
                        return BadRequest(new { message = $"Số lượng tồn kho của sản phẩm {sanPham.TenSanPham} không đủ (còn {sanPham.SoLuong})" });
                    }
                }
            }

            // Nếu số lượng đủ, cập nhật tồn kho
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var chiTiet in order.ChiTietDonHangs)
                    {
                        if (chiTiet.MaCombo != null)
                        {
                            // Cập nhật số lượng tồn kho của combo
                            var combo = chiTiet.MaComboNavigation;
                            if (combo != null)
                            {
                                combo.SoLuong -= chiTiet.SoLuong;
                            }

                            // Cập nhật số lượng tồn kho của các sản phẩm trong combo
                            foreach (var chiTietCombo in combo.ChiTietComBos)
                            {
                                // Tìm sản phẩm có mã bắt đầu bằng baseProductCode
                                var baseProductCode = chiTietCombo.MaSanPham;
                                var sanPham = await _context.SanPhams
                                    .FirstOrDefaultAsync(p => p.MaSanPham.StartsWith(baseProductCode));

                                if (sanPham != null)
                                {
                                    int soLuongCan = (chiTietCombo.SoLuong ?? 0) * (chiTiet.SoLuong ?? 0);
                                    sanPham.SoLuong -= soLuongCan;
                                }
                            }
                        }
                        else
                        {
                            // Cập nhật số lượng tồn kho của sản phẩm
                            var sanPham = await _context.SanPhams
                                .FirstOrDefaultAsync(p => p.MaSanPham == chiTiet.MaSanPham);

                            if (sanPham != null)
                            {
                                sanPham.SoLuong -= chiTiet.SoLuong;
                            }
                        }
                    }

                    // Cập nhật trạng thái đơn hàng
                    order.TrangThaiDonHang = (TrangThaiDonHang)((int)order.TrangThaiDonHang + 1);
                    if (order.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang)
                    {
                        order.TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { message = "Duyệt đơn thành công" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
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