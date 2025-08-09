using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using System.Security.Claims;

namespace UltraStrore.Controllers
{
    [Authorize(Roles = "admin,staff")]
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

            // Lấy tất cả sản phẩm một lần để tránh N+1 query problem
            var allSanPhams = await _context.SanPhams
                .Include(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

            var orders = await _context.DonHangs
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaSanPhamNavigation)
                    .ThenInclude(sp => sp.HinhAnhs)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaComboNavigation)
                    .ThenInclude(c => c.ChiTietComBos)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                    .ThenInclude(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

            var result = orders.Select(d => new
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
                HoTenKhachHang = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.HoTen : null,

                // Chi tiết sản phẩm cho admin
                ChiTietSanPhams = d.ChiTietDonHangs.Select(cd => new
                {
                    MaChiTietDh = cd.MaCtdh,
                    LaCombo = cd.MaCombo != null,
                    TenSanPham = cd.MaCombo != null
        ? (cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại")
        : (cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại"),
                    SoLuong = cd.SoLuong,
                    Gia = cd.Gia,
                    ThanhTien = cd.ThanhTien,
                    MaCombo = cd.MaCombo,
                    MaSanPham = cd.MaSanPham, // ĐẢM BẢO field này được trả về

                    // Thêm thông tin màu sắc và kích thước parsed từ MaSanPham
                    MauSac = cd.MaSanPham != null ? ParseColorFromProductId(cd.MaSanPham) : null,
                    KichThuoc = cd.MaSanPham != null ? ParseSizeFromProductId(cd.MaSanPham) : null,

                    // Hình ảnh sản phẩm - FIXED logic
                    HinhAnh = cd.MaCombo != null
        ? (cd.MaComboNavigation != null && cd.MaComboNavigation.ChiTietComBos.Any()
            ? GetImageByProductId(cd.MaComboNavigation.ChiTietComBos.FirstOrDefault().MaSanPham, allSanPhams)
            : null)
        : GetImageByProductId(cd.MaSanPham, allSanPhams),

                    // Chi tiết combo (nếu là combo)
                    Combo = cd.MaCombo != null && cd.MaComboNavigation != null ? new
                    {
                        TenCombo = cd.MaComboNavigation.TenComBo,
                        GiaCombo = cd.MaComboNavigation.TongGia,
                        SanPhamsTrongCombo = cd.MaComboNavigation.ChiTietComBos.Select(ct => new
                        {
                            TenSanPham = GetProductNameByCode(ct.MaSanPham, allSanPhams),
                            SoLuong = ct.SoLuong,
                            Gia = GetProductPriceByCode(ct.MaSanPham, allSanPhams),
                            ThanhTien = GetProductPriceByCode(ct.MaSanPham, allSanPhams) * ct.SoLuong,
                            MaSanPham = _context.DonHangSupports.Where(g => g.MaChiTietCombo == ct.MaChiTietComBo && g.ChiTietGioHang == cd.MaCtdh).Select(g => g.MaSanPham).FirstOrDefault(),
                            // FIXED: Thêm màu sắc và kích thước cho sản phẩm trong combo
                            // Dùng FindMatchingProductInOrder để tìm sản phẩm phù hợp trong đơn hàng
                            MauSac = FindMatchingProductInOrder(cd.MaSanPham, ct.MaSanPham, "color"),
                            KichThuoc = FindMatchingProductInOrder(cd.MaSanPham, ct.MaSanPham, "size"),
                            HinhAnh = GetImageByProductId(ct.MaSanPham, allSanPhams)
                        }).ToList()
                    } : null
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdersByUserId(string id)
        {
            if (string.IsNullOrEmpty(id) || id == "undefined")
            {
                return BadRequest(new { message = "ID người dùng không hợp lệ." });
            }

            // Load all products for efficient lookup
            var allSanPhams = await _context.SanPhams
                .Include(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

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
                    NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : DateTime.UtcNow.ToString("dd/MM/yyyy"),
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
                        MauSac = ParseColorFromProductId(cd.MaSanPham),
                        KichThuoc = ParseSizeFromProductId(cd.MaSanPham),
                        HinhAnh = GetImageByProductId(cd.MaSanPham, allSanPhams),
                        Combo = cd.MaCombo != null && cd.MaComboNavigation != null ? new
                        {
                            TenCombo = cd.MaComboNavigation.TenComBo,
                            GiaCombo = cd.MaComboNavigation.TongGia,
                            SanPhamsTrongCombo = cd.MaComboNavigation.ChiTietComBos.Select(ct => new
                            {
                                TenSanPham = GetProductNameByCode(ct.MaSanPham, allSanPhams),
                                SoLuong = ct.SoLuong,
                                Gia = GetProductPriceByCode(ct.MaSanPham, allSanPhams),
                                ThanhTien = GetProductPriceByCode(ct.MaSanPham, allSanPhams) * ct.SoLuong,
                                MaSanPham = _context.DonHangSupports.Where(g => g.MaChiTietCombo == ct.MaChiTietComBo && g.ChiTietGioHang == cd.MaCtdh).Select(g => g.MaSanPham).FirstOrDefault(),
                                MauSac = FindMatchingProductInOrder(cd.MaSanPham, ct.MaSanPham, "color"),
                                KichThuoc = FindMatchingProductInOrder(cd.MaSanPham, ct.MaSanPham, "size"),
                                HinhAnh = GetImageByProductId(ct.MaSanPham, allSanPhams)
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

            return Ok(ordersQuery);
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
        // GET: api/orders/cancelled
        [HttpGet("cancelled")]
        public async Task<IActionResult> GetCancelledOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            // Lấy tất cả sản phẩm một lần để tránh N+1 query problem
            var allSanPhams = await _context.SanPhams
                .Include(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

            // Đếm tổng số đơn hàng đã hủy
            var totalCancelledOrders = await _context.DonHangs
                .Where(d => d.TrangThaiDonHang == Data.TrangThaiDonHang.DaHuy)
                .CountAsync();

            // Lấy danh sách đơn hàng đã hủy với phân trang
            var cancelledOrders = await _context.DonHangs
                .Where(d => d.TrangThaiDonHang == Data.TrangThaiDonHang.DaHuy)
                .Include(d => d.MaNguoiDungNavigation)
                .Include(d => d.MaNhanVienNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaSanPhamNavigation)
                    .ThenInclude(sp => sp.HinhAnhs)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaComboNavigation)
                    .ThenInclude(c => c.ChiTietComBos)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                    .ThenInclude(sp => sp.HinhAnhs)
                .OrderByDescending(d => d.NgayDat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var result = cancelledOrders.Select(d => new
            {
                MaDonHang = d.MaDonHang,
                TenNguoiNhan = d.TenNguoiNhan,
                NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "",
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

                // Tổng số sản phẩm trong đơn hàng
                TongSoSanPham = d.ChiTietDonHangs.Sum(cd => cd.SoLuong ?? 0),

                // Thông tin nhân viên xử lý (nếu có)
                MaNhanVien = d.MaNhanVien,
                HoTenNhanVien = d.MaNhanVienNavigation != null ? d.MaNhanVienNavigation.HoTen : "Chưa có nhân viên xử lý",

                // Thông tin khách hàng đặt hàng
                MaNguoiDung = d.MaNguoiDung,
                HoTenKhachHang = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.HoTen : "Khách hàng không tồn tại",

                // Thông tin liên hệ
                DiaChi = d.DiaChi,
                SoDienThoai = d.Sdt,

                // Chi tiết sản phẩm trong đơn hàng đã hủy
                ChiTietSanPhams = d.ChiTietDonHangs.Select(cd => new
                {
                    MaChiTietDh = cd.MaCtdh,
                    LaCombo = cd.MaCombo != null,
                    TenSanPham = cd.MaCombo != null
                        ? (cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại")
                        : (cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại"),
                    SoLuong = cd.SoLuong,
                    Gia = cd.Gia,
                    ThanhTien = cd.ThanhTien,
                    MaCombo = cd.MaCombo,
                    MaSanPham = cd.MaSanPham,

                    // Thông tin màu sắc và kích thước
                    MauSac = cd.MaSanPham != null ? ParseColorFromProductId(cd.MaSanPham) : null,
                    KichThuoc = cd.MaSanPham != null ? ParseSizeFromProductId(cd.MaSanPham) : null,

                    // Hình ảnh sản phẩm
                    HinhAnh = cd.MaCombo != null
                        ? (cd.MaComboNavigation != null && cd.MaComboNavigation.ChiTietComBos.Any()
                            ? GetImageByProductId(cd.MaComboNavigation.ChiTietComBos.FirstOrDefault().MaSanPham, allSanPhams)
                            : null)
                        : GetImageByProductId(cd.MaSanPham, allSanPhams),

                    // Chi tiết combo (nếu là combo)
                    Combo = cd.MaCombo != null && cd.MaComboNavigation != null ? new
                    {
                        TenCombo = cd.MaComboNavigation.TenComBo,
                        GiaCombo = cd.MaComboNavigation.TongGia,
                        SanPhamsTrongCombo = cd.MaComboNavigation.ChiTietComBos.Select(ct => new
                        {
                            TenSanPham = GetProductNameByCode(ct.MaSanPham, allSanPhams),
                            SoLuong = ct.SoLuong,
                            Gia = GetProductPriceByCode(ct.MaSanPham, allSanPhams),
                            ThanhTien = GetProductPriceByCode(ct.MaSanPham, allSanPhams) * ct.SoLuong,
                            MaSanPham = ct.MaSanPham,
                            MauSac = FindMatchingProductInOrder(cd.MaSanPham, ct.MaSanPham, "color"),
                            KichThuoc = FindMatchingProductInOrder(cd.MaSanPham, ct.MaSanPham, "size"),
                            HinhAnh = GetImageByProductId(ct.MaSanPham, allSanPhams)
                        }).ToList()
                    } : null
                }).ToList()
            }).ToList();

            // Tính toán thông tin phân trang
            var totalPages = (int)Math.Ceiling((double)totalCancelledOrders / pageSize);
            var hasNextPage = page < totalPages;
            var hasPreviousPage = page > 1;

            var response = new
            {
                Data = result,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalCancelledOrders,
                    TotalPages = totalPages,
                    HasNextPage = hasNextPage,
                    HasPreviousPage = hasPreviousPage
                },
                Summary = new
                {
                    TotalCancelledOrders = totalCancelledOrders,
                    TotalCancelledAmount = result.Sum(r => r.TongTien),
                    TotalFinalAmount = result.Sum(r => r.FinalAmount ?? 0)
                }
            };

            return Ok(response);
        }


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

        private string ParseColorFromProductId(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            var parts = productId.Split('_');
            if (parts.Length >= 2)
            {
                var colorCode = parts[1].ToLower();
                var colorMap = new Dictionary<string, string>
                {
                    {"ff0000", "Đỏ"},
                    {"0000ff", "Xanh dương"},
                    {"00ff00", "Xanh lá"},
                    {"ffffff", "Trắng"},
                    {"000000", "Đen"},
                    {"ff00ff", "Hồng"},
                    {"0c06f5", "Xanh navy"},
                    {"ffff00", "Vàng"},
                    {"ffa500", "Cam"},
                    {"800080", "Tím"},
                    {"a52a2a", "Nâu"},
                    {"808080", "Xám"},
                    {"c0c0c0", "Bạc"},
                    {"ffc0cb", "Hồng nhạt"}
                };
                return colorMap.ContainsKey(colorCode) ? colorMap[colorCode] : $"#{colorCode}";
            }
            return null;
        }

        private string ParseSizeFromProductId(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            var parts = productId.Split('_');
            if (parts.Length >= 3)
            {
                var sizeCode = parts[2];
                var sizeMap = new Dictionary<string, string>
                {
                    {"S", "S"},
                    {"M", "M"},
                    {"L", "L"},
                    {"XL", "XL"},
                    {"XXL", "XXL"},
                    {"XXXL", "XXXL"},
                    {"SA", "S"},
                    {"MA", "M"},
                    {"LA", "L"},
                    {"XLA", "XL"},
                    {"XXLA", "XXL"},
                    {"XXXLA", "XXXL"}
                };
                return sizeMap.ContainsKey(sizeCode) ? sizeMap[sizeCode] : sizeCode;
            }
            return null;
        }

        // IMPROVED METHOD: Tìm sản phẩm phù hợp trong đơn hàng dựa trên base product code
        private string FindMatchingProductInOrder(string orderProductId, string baseProductId, string attributeType)
        {
            if (string.IsNullOrEmpty(orderProductId) || string.IsNullOrEmpty(baseProductId))
                return null;

            // Lấy base code của sản phẩm trong combo (VD: A00001 từ A00001)
            var baseProductCode = baseProductId.Contains("_") ? baseProductId.Split('_')[0] : baseProductId;

            // Parse order product ID để tìm tất cả các sản phẩm có trong đó
            // orderProductId có thể chứa nhiều sản phẩm như: "Q00001_000000_XL" hoặc phức tạp hơn

            // Tách orderProductId thành các phần
            var orderParts = orderProductId.Split('_');

            // Tìm vị trí của baseProductCode trong orderProductId
            for (int i = 0; i < orderParts.Length; i++)
            {
                if (orderParts[i] == baseProductCode)
                {
                    // Tìm thấy base product code, bây giờ tìm color và size
                    if (attributeType == "color" && i + 1 < orderParts.Length)
                    {
                        // Color nằm ngay sau product code
                        var colorCode = orderParts[i + 1].ToLower();
                        return GetColorNameFromCode(colorCode);
                    }
                    else if (attributeType == "size" && i + 2 < orderParts.Length)
                    {
                        // Size nằm sau color
                        var sizeCode = orderParts[i + 2];
                        return GetSizeNameFromCode(sizeCode);
                    }
                }
            }

            return null;
        }

        private string GetColorNameFromCode(string colorCode)
        {
            var colorMap = new Dictionary<string, string>
    {
        {"ff0000", "Đỏ"},
        {"0000ff", "Xanh dương"},
        {"00ff00", "Xanh lá"},
        {"ffffff", "Trắng"},
        {"000000", "Đen"},
        {"ff00ff", "Hồng"},
        {"0c06f5", "Xanh navy"},
        {"ffff00", "Vàng"},
        {"ffa500", "Cam"},
        {"800080", "Tím"},
        {"a52a2a", "Nâu"},
        {"808080", "Xám"},
        {"c0c0c0", "Bạc"},
        {"ffc0cb", "Hồng nhạt"}
    };
            return colorMap.ContainsKey(colorCode) ? colorMap[colorCode] : $"#{colorCode}";
        }

        private string GetSizeNameFromCode(string sizeCode)
        {
            var sizeMap = new Dictionary<string, string>
    {
        {"S", "S"},
        {"M", "M"},
        {"L", "L"},
        {"XL", "XL"},
        {"XXL", "XXL"},
        {"XXXL", "XXXL"},
        {"SA", "S"},
        {"MA", "M"},
        {"LA", "L"},
        {"XLA", "XL"},
        {"XXLA", "XXL"},
        {"XXXLA", "XXXL"}
    };
            return sizeMap.ContainsKey(sizeCode) ? sizeMap[sizeCode] : sizeCode;
        }

        // DEPRECATED: Keep for backward compatibility but not used anymore
        private string GetProductColorFromOrder(string orderProductId, string baseProductId)
        {
            return FindMatchingProductInOrder(orderProductId, baseProductId, "color");
        }

        // DEPRECATED: Keep for backward compatibility but not used anymore
        private string GetProductSizeFromOrder(string orderProductId, string baseProductId)
        {
            return FindMatchingProductInOrder(orderProductId, baseProductId, "size");
        }

        // FIXED: Enhanced image lookup method - Changed parameter type
        private string GetImageByProductId(string productId, List<SanPham> allSanPhams = null)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            // Method 1: Direct lookup by exact product ID
            var exactMatch = _context.HinhAnhs
                .Where(ha => ha.MaSanPham == productId)
                .Select(ha => ha.Link)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(exactMatch))
                return exactMatch;

            // Method 2: Lookup by base product code (lấy phần đầu trước dấu _)
            var baseProductId = productId.Contains("_") ? productId.Split('_')[0] : productId;

            // Tìm trong database theo base product code
            var baseMatch = _context.HinhAnhs
                .Where(ha => ha.MaSanPham.StartsWith(baseProductId + "_") || ha.MaSanPham == baseProductId)
                .Select(ha => ha.Link)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(baseMatch))
                return baseMatch;

            // Method 3: Use preloaded data if available
            if (allSanPhams != null)
            {
                var productWithImage = allSanPhams.FirstOrDefault(sp =>
                    sp.MaSanPham == productId ||
                    sp.MaSanPham == baseProductId ||
                    sp.MaSanPham.StartsWith(baseProductId + "_"));

                return productWithImage?.HinhAnhs?.FirstOrDefault()?.Link;
            }

            return null;
        }

        // FIXED: Get product name by product code - Changed parameter type
        private string GetProductNameByCode(string productCode, List<SanPham> allSanPhams)
        {
            if (string.IsNullOrEmpty(productCode) || allSanPhams == null)
                return "Sản phẩm không tồn tại";

            // Lấy base product code (phần trước dấu _ đầu tiên)
            var baseProductId = productCode.Contains("_") ? productCode.Split('_')[0] : productCode;

            // Tìm sản phẩm theo base code
            var product = allSanPhams.FirstOrDefault(sp =>
                sp.MaSanPham == productCode ||
                sp.MaSanPham == baseProductId ||
                sp.MaSanPham.StartsWith(baseProductId + "_"));

            return product?.TenSanPham ?? "Sản phẩm không tồn tại";
        }

        // FIXED: Get product price by product code - Changed parameter type
        private decimal GetProductPriceByCode(string productCode, List<SanPham> allSanPhams)
        {
            if (string.IsNullOrEmpty(productCode) || allSanPhams == null)
                return 0;

            // Lấy base product code
            var baseProductId = productCode.Contains("_") ? productCode.Split('_')[0] : productCode;

            // Tìm sản phẩm theo base code
            var product = allSanPhams.FirstOrDefault(sp =>
                sp.MaSanPham == productCode ||
                sp.MaSanPham == baseProductId ||
                sp.MaSanPham.StartsWith(baseProductId + "_"));

            return product?.Gia ?? 0;
        }
    }
}