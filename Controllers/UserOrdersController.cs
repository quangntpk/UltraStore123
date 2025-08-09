using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UltraStrore.Data;
using UltraStrore.Helper;
using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Controllers
{
    [Route("api/user/orders")]
    [ApiController]
    //[Authorize]
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
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng. Vui lòng đăng nhập lại." });
            }

            // ✅ FIXED: Preload all necessary data including DonHangSupports
            var allSanPhams = await _context.SanPhams
                .Include(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

            var allDonHangSupports = await _context.DonHangSupports
                .AsNoTracking()
                .ToListAsync();

            var orders = await _context.DonHangs
                .Where(d => d.MaNguoiDung == currentUserId)
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
                    Id = "ORD-" + d.MaDonHang.ToString("D5"),
                    Date = d.NgayDat != null ? d.NgayDat.Value.ToString("yyyy-MM-dd") : "",
                    Status = d.TrangThaiDonHang == TrangThaiDonHang.ChuaXacNhan ? "pending" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangXuLy ? "processing" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangGiaoHang ? "shipping" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "completed" : "canceled",
                    Total = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    FinalAmount = d.FinalAmount,
                    DiscountAmount = d.DiscountAmount,
                    ShippingFee = d.ShippingFee,

                    // ✅ FIXED: Payment fields - thêm logic thanh toán đúng
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    PaymentStatus = GetPaymentStatus(d.TrangThaiHang, d.TrangThaiDonHang),
                    IsPaymentCompleted = IsPaymentCompleted(d.TrangThaiHang, d.TrangThaiDonHang),

                    // ✅ FIXED: Lấy Items từ DonHangSupports
                    Items = d.ChiTietDonHangs.Select(cd => GetOrderItemFromSupport(cd, allDonHangSupports, allSanPhams)).ToList(),
                    TenNguoiNhan = d.TenNguoiNhan,
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

            // ✅ FIXED: Preload all necessary data including DonHangSupports
            var allSanPhams = await _context.SanPhams
                .Include(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

            var allDonHangSupports = await _context.DonHangSupports
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
                .ToListAsync();

            if (ordersQuery == null || !ordersQuery.Any())
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng nào cho người dùng này." });
            }

            var orders = ordersQuery.Select(d => new
            {
                MaDonHang = d.MaDonHang,
                TenNguoiNhan = d.TenNguoiNhan,
                NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : DateTime.UtcNow.ToString("dd/MM/yyyy"),
                TrangThaiDonHang = (int)d.TrangThaiDonHang,
                TrangThaiThanhToan = (int)d.TrangThaiHang,
                HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",

                // ✅ FIXED: Payment Status fields
                PaymentStatus = GetPaymentStatus(d.TrangThaiHang, d.TrangThaiDonHang),
                IsPaymentCompleted = IsPaymentCompleted(d.TrangThaiHang, d.TrangThaiDonHang),
                PaymentStatusText = GetPaymentStatusText(d.TrangThaiHang, d.TrangThaiDonHang),

                LyDoHuy = d.LyDoHuy,
                TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien),
                FinalAmount = d.FinalAmount,
                DiscountAmount = d.DiscountAmount,
                ShippingFee = d.ShippingFee,
                // ✅ FIXED: Sản phẩm từ DonHangSupports
                SanPhams = d.ChiTietDonHangs.Select(cd => GetDetailedOrderItemFromSupport(cd, allDonHangSupports, allSanPhams)).ToList(),
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

                    // ✅ FIXED: Payment Display Logic
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "Thanh toán khi nhận hàng" : "Thanh toán VNPay",
                    TrangThaiThanhToan = GetPaymentStatusText(d.TrangThaiHang, d.TrangThaiDonHang),

                    TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien),
                    SoTienGiam = d.DiscountAmount,
                    PhiGiaoHang = d.ShippingFee,
                    ThanhTienCuoiCung = d.FinalAmount
                }
            }).ToList();

            return Ok(orders);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUserOrders([FromQuery] string query)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng. Vui lòng đăng nhập lại." });
            }

            if (string.IsNullOrEmpty(query))
            {
                return await GetOrdersByUserId(currentUserId);
            }

            // ✅ FIXED: Preload DonHangSupports
            var allSanPhams = await _context.SanPhams
                .Include(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

            var allDonHangSupports = await _context.DonHangSupports
                .AsNoTracking()
                .ToListAsync();

            // ✅ ENHANCED: Tìm kiếm mở rộng - bao gồm cả tên sản phẩm
            var orders = await _context.DonHangs
                .Where(d => d.MaNguoiDung == currentUserId)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .Where(d =>
                    // Tìm theo mã đơn hàng
                    d.MaDonHang.ToString().Contains(query) ||
                    // Tìm theo tên người nhận
                    (d.TenNguoiNhan != null && d.TenNguoiNhan.Contains(query)) ||
                    // Tìm theo số điện thoại
                    (d.Sdt != null && d.Sdt.Contains(query)) ||
                    // ✅ NEW: Tìm theo tên sản phẩm trong đơn hàng
                    d.ChiTietDonHangs.Any(cd =>
                        (cd.MaSanPhamNavigation != null && cd.MaSanPhamNavigation.TenSanPham.Contains(query)) ||
                        (cd.MaComboNavigation != null && cd.MaComboNavigation.TenComBo.Contains(query))
                    )
                )
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng nào khớp với tiêu chí tìm kiếm." });
            }

            var result = orders.Select(d => new
            {
                MaDonHang = d.MaDonHang,
                TenNguoiNhan = d.TenNguoiNhan,
                NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : DateTime.UtcNow.ToString("dd/MM/yyyy"),
                TrangThaiDonHang = (int)d.TrangThaiDonHang,
                TrangThaiThanhToan = (int)d.TrangThaiHang,
                HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                PaymentStatus = GetPaymentStatus(d.TrangThaiHang, d.TrangThaiDonHang),
                IsPaymentCompleted = IsPaymentCompleted(d.TrangThaiHang, d.TrangThaiDonHang),
                PaymentStatusText = GetPaymentStatusText(d.TrangThaiHang, d.TrangThaiDonHang),
                LyDoHuy = d.LyDoHuy,
                TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien),
                FinalAmount = d.FinalAmount,
                DiscountAmount = d.DiscountAmount,
                ShippingFee = d.ShippingFee,
                SanPhams = d.ChiTietDonHangs.Select(cd => GetDetailedOrderItemFromSupport(cd, allDonHangSupports, allSanPhams)).ToList(),
                ThongTinNguoiDung = new
                {
                    TenNguoiNhan = d.TenNguoiNhan,
                    DiaChi = d.DiaChi,
                    Sdt = d.Sdt,
                    TenNguoiDat = d.MaNguoiDungNavigation?.HoTen
                },
                ThongTinDonHang = new
                {
                    NgayDat = d.NgayDat != null ? d.NgayDat.Value.ToString("dd/MM/yyyy") : DateTime.UtcNow.ToString("dd/MM/yyyy"),
                    TrangThai = (int)d.TrangThaiDonHang,
                    ThanhToan = (int)d.TrangThaiHang,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "Thanh toán khi nhận hàng" : "Thanh toán VNPay",
                    TrangThaiThanhToan = GetPaymentStatusText(d.TrangThaiHang, d.TrangThaiDonHang),
                    TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien),
                    SoTienGiam = d.DiscountAmount,
                    PhiGiaoHang = d.ShippingFee,
                    ThanhTienCuoiCung = d.FinalAmount
                }
            }).ToList();

            return Ok(result);
        }

        // ✅ NEW: Helper methods cho Payment Status Logic
        private string GetPaymentStatus(TrangThaiThanhToan trangThaiHang, TrangThaiDonHang trangThaiDonHang)
        {
            // VNPay orders are always paid when created
            if (trangThaiHang == TrangThaiThanhToan.ThanhToanVNPay)
            {
                return "paid";
            }

            // COD orders are only paid when delivered
            if (trangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang)
            {
                return trangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "paid" : "unpaid";
            }

            return "unpaid";
        }

        private bool IsPaymentCompleted(TrangThaiThanhToan trangThaiHang, TrangThaiDonHang trangThaiDonHang)
        {
            // VNPay orders are always paid
            if (trangThaiHang == TrangThaiThanhToan.ThanhToanVNPay)
            {
                return true;
            }

            // COD orders are only paid when delivered
            if (trangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang)
            {
                return trangThaiDonHang == TrangThaiDonHang.DaGiaoHang;
            }

            return false;
        }

        private string GetPaymentStatusText(TrangThaiThanhToan trangThaiHang, TrangThaiDonHang trangThaiDonHang)
        {
            // VNPay orders are always paid
            if (trangThaiHang == TrangThaiThanhToan.ThanhToanVNPay)
            {
                return "Đã thanh toán";
            }

            // COD orders are only paid when delivered
            if (trangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang)
            {
                return trangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "Đã thanh toán" : "Chưa thanh toán";
            }

            return "Chưa thanh toán";
        }

        // ✅ THÊM: Debug method để kiểm tra dữ liệu DonHangSupports
        [HttpGet("debug/{orderId}")]
        public async Task<IActionResult> DebugOrder(int orderId)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

            if (donHang == null)
            {
                return NotFound("Order not found");
            }

            var allSupports = await _context.DonHangSupports.ToListAsync();

            var debugInfo = new
            {
                OrderId = orderId,
                PaymentMethod = donHang.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                OrderStatus = (int)donHang.TrangThaiDonHang,
                PaymentStatus = GetPaymentStatus(donHang.TrangThaiHang, donHang.TrangThaiDonHang),
                IsPaymentCompleted = IsPaymentCompleted(donHang.TrangThaiHang, donHang.TrangThaiDonHang),
                PaymentStatusText = GetPaymentStatusText(donHang.TrangThaiHang, donHang.TrangThaiDonHang),
                ChiTietDonHangs = donHang.ChiTietDonHangs.Select(cd => new
                {
                    MaCtdh = cd.MaCtdh,
                    MaCombo = cd.MaCombo,
                    MaSanPham = cd.MaSanPham,
                    SoLuong = cd.SoLuong,
                    // ✅ Tìm supports theo ChiTietGioHang = MaCtdh
                    RelatedSupports = allSupports.Where(s => s.ChiTietGioHang == cd.MaCtdh).Select(s => new
                    {
                        s.ID,
                        s.MaSanPham,
                        s.ChiTietGioHang,
                        s.MaChiTietCombo,
                        s.SoLuong,
                        s.Version
                    }).ToList(),
                    // ✅ Chi tiết combo để mapping MaChiTietCombo
                    ComboDetails = cd.MaComboNavigation?.ChiTietComBos?.Select(ccb => new
                    {
                        MaChiTietComBo = ccb.MaChiTietComBo,
                        MaSanPham = ccb.MaSanPham,
                        SoLuong = ccb.SoLuong,
                        // ✅ Tìm support theo MaChiTietCombo
                        MatchingSupports = allSupports.Where(s => s.MaChiTietCombo == ccb.MaChiTietComBo && s.ChiTietGioHang == cd.MaCtdh).Select(s => new
                        {
                            s.ID,
                            s.MaSanPham,
                            s.SoLuong
                        }).ToList(),
                        // ✅ FIXED: Tìm latest supports nếu ChiTietGioHang = 0
                        LatestMatchingSupports = allSupports.Where(s => s.MaChiTietCombo == ccb.MaChiTietComBo && s.ChiTietGioHang == 0)
                            .OrderByDescending(s => s.ID).Take(3).Select(s => new
                            {
                                s.ID,
                                s.MaSanPham,
                                s.SoLuong
                            }).ToList()
                    }).ToList()
                }).ToList(),
                // ✅ Tất cả supports có ChiTietGioHang = 0 (chưa được assign)
                UnassignedSupports = allSupports.Where(s => s.ChiTietGioHang == 0).Select(s => new
                {
                    s.ID,
                    s.MaSanPham,
                    s.ChiTietGioHang,
                    s.MaChiTietCombo,
                    s.SoLuong,
                    s.Version
                }).ToList(),
                // ✅ Latest supports (có thể là của đơn hàng này)
                LatestSupports = allSupports.OrderByDescending(s => s.ID).Take(10).Select(s => new
                {
                    s.ID,
                    s.MaSanPham,
                    s.ChiTietGioHang,
                    s.MaChiTietCombo,
                    s.SoLuong,
                    s.Version
                }).ToList()
            };

            return Ok(debugInfo);
        }

        // ✅ NEW: Helper method để lấy Order Item từ DonHangSupports
        private object GetOrderItemFromSupport(ChiTietDonHang cd, List<DonHangSupport> allDonHangSupports, List<SanPham> allSanPhams)
        {
            // ✅ FIXED: Lấy tất cả DonHangSupports tương ứng với ChiTietDonHang này
            var supports = allDonHangSupports.Where(s => s.ChiTietGioHang == cd.MaCtdh).ToList();

            // ✅ FIXED: Nếu không có supports với ChiTietGioHang đúng, tìm latest supports
            if (!supports.Any() && cd.MaCombo != null && cd.MaComboNavigation?.ChiTietComBos != null)
            {
                // Lấy latest supports có MaChiTietCombo matching và ChiTietGioHang = 0
                var comboItemIds = cd.MaComboNavigation.ChiTietComBos.Select(ccb => ccb.MaChiTietComBo).ToList();
                var latestSupports = allDonHangSupports.Where(s => s.ChiTietGioHang == 0 && comboItemIds.Contains(s.MaChiTietCombo))
                    .GroupBy(s => s.MaChiTietCombo)
                    .Select(g => g.OrderByDescending(s => s.ID).First())
                    .ToList();

                supports = latestSupports;
            }

            if (supports.Any())
            {
                // Lấy sản phẩm đầu tiên từ DonHangSupports để hiển thị
                var firstSupport = supports.First();
                return new
                {
                    Id = cd.MaCtdh,
                    Name = cd.MaCombo != null ? cd.MaComboNavigation?.TenComBo : GetProductNameByCode_FIXED(firstSupport.MaSanPham, allSanPhams),
                    Quantity = supports.Sum(s => s.SoLuong), // Tổng số lượng từ tất cả supports
                    Price = cd.Gia,
                    ProductCode = firstSupport.MaSanPham,
                    ComboCode = cd.MaCombo,
                    Image = GetImageByProductId_ENHANCED(firstSupport.MaSanPham, allSanPhams)
                };
            }
            else
            {
                // Fallback về logic cũ nếu không có DonHangSupports
                return new
                {
                    Id = cd.MaCtdh,
                    Name = cd.MaCombo != null ? cd.MaComboNavigation?.TenComBo : cd.MaSanPhamNavigation?.TenSanPham,
                    Quantity = cd.SoLuong,
                    Price = cd.Gia,
                    ProductCode = cd.MaSanPham,
                    ComboCode = cd.MaCombo,
                    Image = cd.MaCombo != null
                        ? GetImageFromCombo_ENHANCED(cd.MaComboNavigation, allSanPhams)
                        : GetImageByProductId_ENHANCED(cd.MaSanPham, allSanPhams)
                };
            }
        }

        // ✅ NEW: Helper method để lấy chi tiết Order Item từ DonHangSupports
        private object GetDetailedOrderItemFromSupport(ChiTietDonHang cd, List<DonHangSupport> allDonHangSupports, List<SanPham> allSanPhams)
        {
            // ✅ FIXED: Lấy tất cả DonHangSupports tương ứng với ChiTietDonHang này
            var supports = allDonHangSupports.Where(s => s.ChiTietGioHang == cd.MaCtdh).ToList();

            // ✅ FIXED: Nếu không có supports với ChiTietGioHang đúng, tìm latest supports
            if (!supports.Any() && cd.MaCombo != null && cd.MaComboNavigation?.ChiTietComBos != null)
            {
                var comboItemIds = cd.MaComboNavigation.ChiTietComBos.Select(ccb => ccb.MaChiTietComBo).ToList();
                var latestSupports = allDonHangSupports.Where(s => s.ChiTietGioHang == 0 && comboItemIds.Contains(s.MaChiTietCombo))
                    .GroupBy(s => s.MaChiTietCombo)
                    .Select(g => g.OrderByDescending(s => s.ID).First())
                    .ToList();

                supports = latestSupports;
            }

            if (supports.Any())
            {
                // Nếu có DonHangSupports, sử dụng dữ liệu từ đó
                var firstSupport = supports.First();
                var actualProductCode = firstSupport.MaSanPham;

                return new
                {
                    MaChiTietDh = cd.MaCtdh,
                    LaCombo = cd.MaCombo != null,
                    TenSanPham = cd.MaCombo != null
                        ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                        : GetProductNameByCode_FIXED(actualProductCode, allSanPhams),
                    SoLuong = cd.SoLuong,
                    Gia = cd.Gia,
                    ThanhTien = cd.ThanhTien,
                    MaCombo = cd.MaCombo,
                    MaSanPham = actualProductCode, // ✅ FIXED: Sử dụng mã sản phẩm thực từ DonHangSupports
                                                   // ✅ FIXED: Lấy màu sắc và kích thước từ sản phẩm thực
                    MauSac = ExtractColorFromProductCode(actualProductCode),
                    KichThuoc = ExtractSizeFromProductCode(actualProductCode),
                    MauSacHex = ExtractColorHexFromProductCode(actualProductCode),
                    HinhAnh = GetImageByProductId_ENHANCED(actualProductCode, allSanPhams),
                    // ✅ FIXED: Combo với dữ liệu thực từ DonHangSupports
                    Combo = cd.MaCombo != null && cd.MaComboNavigation != null
                        ? GetComboDetailsFromSupports(cd, supports, allSanPhams)
                        : null
                };
            }
            else
            {
                // ✅ FIXED: Sửa lỗi type conversion
                List<object> sanPhamsTrongCombo;

                if (cd.MaComboNavigation?.ChiTietComBos != null)
                {
                    sanPhamsTrongCombo = cd.MaComboNavigation.ChiTietComBos.Select(ct => (object)new
                    {
                        TenSanPham = GetProductNameByCode_FIXED(ct.MaSanPham, allSanPhams),
                        SoLuong = ct.SoLuong,
                        Gia = GetProductPriceByCode_FIXED(ct.MaSanPham, allSanPhams),
                        ThanhTien = GetProductPriceByCode_FIXED(ct.MaSanPham, allSanPhams) * (ct.SoLuong ?? 0),
                        MaSanPham = ct.MaSanPham,
                        MauSac = ExtractColorFromProductCode(ct.MaSanPham),
                        KichThuoc = ExtractSizeFromProductCode(ct.MaSanPham),
                        MauSacHex = ExtractColorHexFromProductCode(ct.MaSanPham),
                        HinhAnh = GetImageByProductId_ENHANCED(ct.MaSanPham, allSanPhams)
                    }).ToList();
                }
                else
                {
                    sanPhamsTrongCombo = new List<object>();
                }

                // Fallback về logic cũ nếu không có DonHangSupports
                return new
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
                    MauSac = ExtractColorFromProductCode(cd.MaSanPham),
                    KichThuoc = ExtractSizeFromProductCode(cd.MaSanPham),
                    MauSacHex = ExtractColorHexFromProductCode(cd.MaSanPham),
                    HinhAnh = cd.MaCombo != null
                        ? GetImageFromCombo_ENHANCED(cd.MaComboNavigation, allSanPhams)
                        : GetImageByProductId_ENHANCED(cd.MaSanPham, allSanPhams),
                    Combo = cd.MaCombo != null && cd.MaComboNavigation != null ? new
                    {
                        TenCombo = cd.MaComboNavigation.TenComBo,
                        GiaCombo = cd.MaComboNavigation.TongGia,
                        SanPhamsTrongCombo = sanPhamsTrongCombo
                    } : null
                };
            }
        }

        // ✅ FIXED: Helper method để lấy chi tiết combo từ DonHangSupports
        private object GetComboDetailsFromSupports(ChiTietDonHang cd, List<DonHangSupport> supports, List<SanPham> allSanPhams)
        {
            try
            {
                var comboProducts = new List<object>();

                if (cd.MaComboNavigation?.ChiTietComBos != null)
                {
                    foreach (var chiTietCombo in cd.MaComboNavigation.ChiTietComBos)
                    {
                        // ✅ FIXED: Tìm support tương ứng với MaChiTietCombo
                        var correspondingSupport = supports.FirstOrDefault(s => s.MaChiTietCombo == chiTietCombo.MaChiTietComBo);

                        if (correspondingSupport != null)
                        {
                            // ✅ FIXED: Sử dụng thông tin từ DonHangSupports
                            var actualProductCode = correspondingSupport.MaSanPham;
                            var actualQuantity = correspondingSupport.SoLuong;

                            comboProducts.Add(new
                            {
                                TenSanPham = GetProductNameByCode_FIXED(actualProductCode, allSanPhams),
                                SoLuong = actualQuantity,
                                Gia = GetProductPriceByCode_FIXED(actualProductCode, allSanPhams),
                                ThanhTien = GetProductPriceByCode_FIXED(actualProductCode, allSanPhams) * actualQuantity,
                                MaSanPham = actualProductCode,
                                MauSac = ExtractColorFromProductCode(actualProductCode),
                                KichThuoc = ExtractSizeFromProductCode(actualProductCode),
                                MauSacHex = ExtractColorHexFromProductCode(actualProductCode),
                                HinhAnh = GetImageByProductId_ENHANCED(actualProductCode, allSanPhams)
                            });
                        }
                        else
                        {
                            // Fallback về thông tin mặc định từ ChiTietComBo
                            comboProducts.Add(new
                            {
                                TenSanPham = GetProductNameByCode_FIXED(chiTietCombo.MaSanPham, allSanPhams),
                                SoLuong = chiTietCombo.SoLuong,
                                Gia = GetProductPriceByCode_FIXED(chiTietCombo.MaSanPham, allSanPhams),
                                ThanhTien = GetProductPriceByCode_FIXED(chiTietCombo.MaSanPham, allSanPhams) * (chiTietCombo.SoLuong ?? 0),
                                MaSanPham = chiTietCombo.MaSanPham,
                                MauSac = ExtractColorFromProductCode(chiTietCombo.MaSanPham),
                                KichThuoc = ExtractSizeFromProductCode(chiTietCombo.MaSanPham),
                                MauSacHex = ExtractColorHexFromProductCode(chiTietCombo.MaSanPham),
                                HinhAnh = GetImageByProductId_ENHANCED(chiTietCombo.MaSanPham, allSanPhams)
                            });
                        }
                    }
                }

                return new
                {
                    TenCombo = cd.MaComboNavigation.TenComBo,
                    GiaCombo = cd.MaComboNavigation.TongGia,
                    SanPhamsTrongCombo = comboProducts
                };
            }
            catch (Exception ex)
            {
                // ✅ FIXED: Fallback về logic cũ với kiểu dữ liệu đúng
                return new
                {
                    TenCombo = cd.MaComboNavigation?.TenComBo ?? "Unknown Combo",
                    GiaCombo = cd.MaComboNavigation?.TongGia ?? 0,
                    SanPhamsTrongCombo = cd.MaComboNavigation?.ChiTietComBos?.Select(ct => (object)new
                    {
                        TenSanPham = GetProductNameByCode_FIXED(ct.MaSanPham, allSanPhams),
                        SoLuong = ct.SoLuong,
                        Gia = GetProductPriceByCode_FIXED(ct.MaSanPham, allSanPhams),
                        ThanhTien = GetProductPriceByCode_FIXED(ct.MaSanPham, allSanPhams) * (ct.SoLuong ?? 0),
                        MaSanPham = ct.MaSanPham,
                        MauSac = ExtractColorFromProductCode(ct.MaSanPham),
                        KichThuoc = ExtractSizeFromProductCode(ct.MaSanPham),
                        MauSacHex = ExtractColorHexFromProductCode(ct.MaSanPham),
                        HinhAnh = GetImageByProductId_ENHANCED(ct.MaSanPham, allSanPhams)
                    }).ToList() ?? new List<object>()
                };
            }
        }

        // PUT: api/user/orders/cancel/{id}
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng. Vui lòng đăng nhập lại." });
                }

                if (string.IsNullOrWhiteSpace(request?.LyDoHuy))
                {
                    return BadRequest(new { message = "Lý do hủy không được để trống" });
                }

                if (request.LyDoHuy.Length > 500)
                {
                    return BadRequest(new { message = "Lý do hủy không được quá 500 ký tự" });
                }

                var order = await _context.DonHangs.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Đơn hàng không tồn tại" });
                }

                if (order.MaNguoiDung != currentUserId)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền hủy đơn hàng này" });
                }

                if (order.TrangThaiDonHang == Data.TrangThaiDonHang.DaHuy)
                {
                    return BadRequest(new { message = "Đơn hàng này đã được hủy trước đó" });
                }

                if (order.TrangThaiDonHang != Data.TrangThaiDonHang.ChuaXacNhan &&
                    order.TrangThaiDonHang != Data.TrangThaiDonHang.DangXuLy)
                {
                    return BadRequest(new { message = "Chỉ có thể hủy đơn hàng khi chưa xác nhận hoặc đang xử lý" });
                }

                var nguoiDung = await _context.NguoiDungs.FindAsync(currentUserId);
                if (nguoiDung == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin người dùng" });
                }

                if (nguoiDung.LockoutEndDate.HasValue && nguoiDung.LockoutEndDate.Value > DateTime.Now)
                {
                    return BadRequest(new
                    {
                        message = "Tài khoản của bạn đang bị khóa do hủy đơn hàng quá nhiều lần",
                        isAccountLocked = true,
                        lockoutMessage = $"Tài khoản sẽ được mở khóa vào {nguoiDung.LockoutEndDate.Value:dd/MM/yyyy HH:mm}"
                    });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    order.TrangThaiDonHang = Data.TrangThaiDonHang.DaHuy;
                    order.LyDoHuy = request.LyDoHuy.Trim();

                    nguoiDung.CancelConunt = (nguoiDung.CancelConunt ?? 0) + 1;

                    bool isAccountLocked = false;
                    string lockoutMessage = "";

                    if (nguoiDung.CancelConunt >= 3)
                    {
                        nguoiDung.LockoutEndDate = DateTime.Now.AddDays(3);
                        nguoiDung.TrangThai = 1;
                        isAccountLocked = true;
                        lockoutMessage = $"Tài khoản đã bị khóa 3 ngày do hủy đơn hàng quá 3 lần. Sẽ được mở khóa vào {nguoiDung.LockoutEndDate.Value:dd/MM/yyyy HH:mm}";
                    }

                    _context.DonHangs.Update(order);
                    _context.NguoiDungs.Update(nguoiDung);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        message = isAccountLocked ?
                            "Đơn hàng đã được hủy thành công. Tài khoản đã bị khóa do hủy đơn hàng quá 3 lần." :
                            $"Hủy đơn thành công. Bạn đã hủy {nguoiDung.CancelConunt} lần.",
                        isAccountLocked = isAccountLocked,
                        lockoutMessage = lockoutMessage,
                        remainingCancellations = isAccountLocked ? 0 : Math.Max(0, 3 - nguoiDung.CancelConunt.Value)
                    });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi khi hủy đơn: {ex.Message}" });
            }
        }

        // Helper method để lấy current user ID
        private string GetCurrentUserId()
        {
            var claims = HttpContext.User?.Claims?.ToList();
            if (claims == null || !claims.Any()) return null;

            var userId = claims.FirstOrDefault(c => c.Type == "MaNguoiDung")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                userId = claims.FirstOrDefault(c => c.Value.StartsWith("ND") || c.Value.StartsWith("KH") || c.Value.StartsWith("AD"))?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
                         claims.FirstOrDefault(c => c.Type == "nameid")?.Value ??
                         claims.FirstOrDefault(c => c.Type == "sub")?.Value ??
                         claims.FirstOrDefault(c => c.Type == "userId")?.Value ??
                         claims.FirstOrDefault(c => c.Type == "id")?.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = HttpContext.User?.Identity?.Name;
            }

            return userId;
        }

        // GET: api/user/orders/bill/{orderId}
        [HttpGet("bill/{orderId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrderByOrderId(int orderId)
        {
            // ✅ FIXED: Preload all data including DonHangSupports
            var allSanPhams = await _context.SanPhams
                .Include(sp => sp.HinhAnhs)
                .AsNoTracking()
                .ToListAsync();

            var allDonHangSupports = await _context.DonHangSupports
                .AsNoTracking()
                .ToListAsync();

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                        .ThenInclude(sp => sp.HinhAnhs)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaComboNavigation)
                        .ThenInclude(c => c.ChiTietComBos)
                            .ThenInclude(ccb => ccb.MaSanPhamNavigation)
                                .ThenInclude(sp => sp.HinhAnhs)
                .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            return Ok(new
            {
                maDonHang = donHang.MaDonHang,
                tenNguoiNhan = donHang.TenNguoiNhan,
                sdt = donHang.Sdt,
                diaChi = donHang.DiaChi,
                finalAmount = donHang.FinalAmount,
                discountAmount = donHang.DiscountAmount,
                shippingFee = donHang.ShippingFee,
                ngayDat = donHang.NgayDat,
                // ✅ FIXED: Payment Status for bill
                hinhThucThanhToan = donHang.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                trangThaiThanhToan = GetPaymentStatusText(donHang.TrangThaiHang, donHang.TrangThaiDonHang),
                isPaymentCompleted = IsPaymentCompleted(donHang.TrangThaiHang, donHang.TrangThaiDonHang),
                // ✅ FIXED: Chi tiết đơn hàng từ DonHangSupports
                chiTietDonHangs = donHang.ChiTietDonHangs.Select(ct => GetBillItemFromSupport(ct, allDonHangSupports, allSanPhams)).ToList()
            });
        }

        // ✅ NEW: Helper method để lấy thông tin cho bill từ DonHangSupports
        private object GetBillItemFromSupport(ChiTietDonHang ct, List<DonHangSupport> allDonHangSupports, List<SanPham> allSanPhams)
        {
            var supports = allDonHangSupports.Where(s => s.ChiTietGioHang == ct.MaCtdh).ToList();

            // ✅ FIXED: Nếu không có supports với ChiTietGioHang đúng, tìm latest supports
            if (!supports.Any() && ct.MaCombo != null && ct.MaComboNavigation?.ChiTietComBos != null)
            {
                var comboItemIds = ct.MaComboNavigation.ChiTietComBos.Select(ccb => ccb.MaChiTietComBo).ToList();
                var latestSupports = allDonHangSupports.Where(s => s.ChiTietGioHang == 0 && comboItemIds.Contains(s.MaChiTietCombo))
                    .GroupBy(s => s.MaChiTietCombo)
                    .Select(g => g.OrderByDescending(s => s.ID).First())
                    .ToList();

                supports = latestSupports;
            }

            if (supports.Any())
            {
                var firstSupport = supports.First();
                var actualProductCode = firstSupport.MaSanPham;

                return new
                {
                    soLuong = ct.SoLuong,
                    gia = ct.Gia,
                    thanhTien = ct.ThanhTien,
                    maSanPham = actualProductCode, // ✅ FIXED: Sử dụng mã sản phẩm thực
                    mauSac = ExtractColorFromProductCode(actualProductCode),
                    kichThuoc = ExtractSizeFromProductCode(actualProductCode),
                    mauSacHex = ExtractColorHexFromProductCode(actualProductCode),
                    hinhAnh = GetImageByProductId_ENHANCED(actualProductCode, allSanPhams),
                    maSanPhamNavigation = new
                    {
                        tenSanPham = GetProductNameByCode_FIXED(actualProductCode, allSanPhams),
                        hinhAnhs = new[] { new {
                            link = GetImageByProductId_ENHANCED(actualProductCode, allSanPhams)
                        } }.Where(x => !string.IsNullOrEmpty(x.link)).ToList()
                    }
                };
            }
            else
            {
                // Fallback về logic cũ
                return new
                {
                    soLuong = ct.SoLuong,
                    gia = ct.Gia,
                    thanhTien = ct.ThanhTien,
                    maSanPham = ct.MaSanPham,
                    mauSac = ExtractColorFromProductCode(ct.MaSanPham),
                    kichThuoc = ExtractSizeFromProductCode(ct.MaSanPham),
                    mauSacHex = ExtractColorHexFromProductCode(ct.MaSanPham),
                    hinhAnh = ct.MaCombo != null
                        ? GetImageFromCombo_ENHANCED(ct.MaComboNavigation, allSanPhams)
                        : GetImageByProductId_ENHANCED(ct.MaSanPham, allSanPhams),
                    maSanPhamNavigation = ct.MaSanPhamNavigation != null ? new
                    {
                        tenSanPham = ct.MaSanPhamNavigation.TenSanPham,
                        hinhAnhs = new[] { new {
                            link = GetImageByProductId_ENHANCED(ct.MaSanPham, allSanPhams)
                        } }.Where(x => !string.IsNullOrEmpty(x.link)).ToList()
                    } : null
                };
            }
        }

        // Helper methods to extract color and size from product code
        private string ExtractColorFromProductCode(string productCode)
        {
            if (string.IsNullOrEmpty(productCode)) return null;

            var parts = productCode.Split('_');
            if (parts.Length >= 2)
            {
                return ConvertHexToColorName(parts[1]);
            }
            return null;
        }

        private string ExtractSizeFromProductCode(string productCode)
        {
            if (string.IsNullOrEmpty(productCode)) return null;

            var parts = productCode.Split('_');
            if (parts.Length >= 3)
            {
                return parts[2];
            }
            return null;
        }

        private string ExtractColorHexFromProductCode(string productCode)
        {
            if (string.IsNullOrEmpty(productCode)) return null;

            var parts = productCode.Split('_');
            if (parts.Length >= 2)
            {
                return "#" + parts[1];
            }
            return null;
        }

        // ✅ FIXED: Cập nhật ConvertHexToColorName với màu 0c06f5
        private string ConvertHexToColorName(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return null;

            // Chuẩn hóa hex string (loại bỏ # nếu có)
            hex = hex.Replace("#", "").ToLower();

            var colorMap = new Dictionary<string, string>
            {
                // Màu cơ bản
                { "ff0000", "Đỏ" },
                { "00ff00", "Xanh lá" },
                { "0000ff", "Xanh dương" },
                { "ffff00", "Vàng" },
                { "ff00ff", "Hồng" },     // ✅ FIXED: ff00ff = Hồng (magenta)
                { "00ffff", "Xanh cyan" },
                { "000000", "Đen" },
                { "ffffff", "Trắng" },
                { "808080", "Xám" },
                { "ffa500", "Cam" },
                { "800080", "Tím" },
                { "a52a2a", "Nâu" },
                { "0c06f5", "Xanh dương" },  // ✅ FIXED: 0c06f5 = Xanh dương
                
                // Thêm các biến thể viết hoa
                { "FF0000", "Đỏ" },
                { "00FF00", "Xanh lá" },
                { "0000FF", "Xanh dương" },
                { "FFFF00", "Vàng" },
                { "FF00FF", "Hồng" },
                { "00FFFF", "Xanh cyan" },
                { "FFFFFF", "Trắng" },
                { "FFA500", "Cam" },
                { "A52A2A", "Nâu" },
                { "0C06F5", "Xanh dương" }
            };

            return colorMap.ContainsKey(hex) ? colorMap[hex] : $"#{hex}";
        }

        private string GetImageByProductId_ENHANCED(string productId, List<SanPham> allSanPhams)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            // Method 1: Direct lookup by exact product ID in database
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
                    (sp.MaSanPham != null && sp.MaSanPham.StartsWith(baseProductId + "_")));

                if (productWithImage?.HinhAnhs?.Any() == true)
                {
                    var image = productWithImage.HinhAnhs.FirstOrDefault();
                    return GetImageUrl(image);
                }
            }

            return null;
        }

        private string GetImageFromCombo_ENHANCED(ComBoSanPham combo, List<SanPham> allSanPhams)
        {
            if (combo?.ChiTietComBos == null || !combo.ChiTietComBos.Any() || allSanPhams == null)
                return null;

            var firstProductInCombo = combo.ChiTietComBos.FirstOrDefault();
            if (firstProductInCombo?.MaSanPham == null)
                return null;

            return GetImageByProductId_ENHANCED(firstProductInCombo.MaSanPham, allSanPhams);
        }

        private string GetProductNameByCode_FIXED(string productCode, List<SanPham> allSanPhams)
        {
            if (string.IsNullOrEmpty(productCode) || allSanPhams == null)
                return "Sản phẩm không tồn tại";

            var baseProductId = productCode.Contains("_") ? productCode.Split('_')[0] : productCode;

            var product = allSanPhams.FirstOrDefault(sp =>
                sp.MaSanPham == productCode ||
                sp.MaSanPham == baseProductId ||
                (sp.MaSanPham != null && sp.MaSanPham.StartsWith(baseProductId + "_")));

            return product?.TenSanPham ?? "Sản phẩm không tồn tại";
        }

        private decimal GetProductPriceByCode_FIXED(string productCode, List<SanPham> allSanPhams)
        {
            if (string.IsNullOrEmpty(productCode) || allSanPhams == null)
                return 0;

            var baseProductId = productCode.Contains("_") ? productCode.Split('_')[0] : productCode;

            var product = allSanPhams.FirstOrDefault(sp =>
                sp.MaSanPham == productCode ||
                sp.MaSanPham == baseProductId ||
                (sp.MaSanPham != null && sp.MaSanPham.StartsWith(baseProductId + "_")));

            return product?.Gia ?? 0;
        }

        private string GetImageUrl(HinhAnh image)
        {
            if (image == null) return "/placeholder.svg";

            if (!string.IsNullOrEmpty(image.Link) && !string.IsNullOrWhiteSpace(image.Link))
            {
                return image.Link;
            }

            if (image.Data != null && image.Data.Length > 0)
            {
                return $"data:image/jpeg;base64,{Convert.ToBase64String(image.Data)}";
            }

            return "/placeholder.svg";
        }

        public class CancelOrderRequest
        {
            [Required(ErrorMessage = "Lý do hủy không được để trống")]
            [StringLength(500, ErrorMessage = "Lý do hủy không được quá 500 ký tự")]
            public string LyDoHuy { get; set; }
        }
    }
}