using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UltraStrore.Data;
using UltraStrore.Helper;
using System.Security.Claims;
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
            // Lấy user ID từ token thay vì hardcode
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Không thể xác thực người dùng. Vui lòng đăng nhập lại." });
            }

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
                    // Rút gọn Items để tránh quá dài
                    Items = d.ChiTietDonHangs.Select(cd => new
                    {
                        Id = cd.MaCtdh,
                        Name = cd.MaCombo != null ? cd.MaComboNavigation.TenComBo : cd.MaSanPhamNavigation.TenSanPham,
                        Quantity = cd.SoLuong,
                        Price = cd.Gia,
                        ProductCode = cd.MaSanPham,
                        ComboCode = cd.MaCombo
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
                LyDoHuy = d.LyDoHuy,
                TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien),
                FinalAmount = d.FinalAmount,
                DiscountAmount = d.DiscountAmount,
                ShippingFee = d.ShippingFee,
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
                    MauSac = cd.MaCombo == null ? ExtractColorFromProductCode(cd.MaSanPham) : null,
                    KichThuoc = cd.MaCombo == null ? ExtractSizeFromProductCode(cd.MaSanPham) : null,
                    MauSacHex = cd.MaCombo == null ? ExtractColorHexFromProductCode(cd.MaSanPham) : null,
                    HinhAnh = cd.MaCombo != null
                        ? GetImageFromCombo(cd.MaCombo)
                        : GetImageFromSanPham(cd.MaSanPham),
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
                            MaSanPham = ct.MaSanPham,
                            MauSac = ExtractColorFromProductCode(ct.MaSanPham),
                            KichThuoc = ExtractSizeFromProductCode(ct.MaSanPham),
                            MauSacHex = ExtractColorHexFromProductCode(ct.MaSanPham),
                            HinhAnh = GetImageFromSanPham(ct.MaSanPham)
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
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "Thanh toán khi nhận hàng" : "Thanh toán VNPay",
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
                return await GetUserOrders();
            }

            var orders = await _context.DonHangs
                .Where(d => d.MaNguoiDung == currentUserId &&
                            (d.MaDonHang.ToString().Contains(query) ||
                             (d.TenNguoiNhan != null && d.TenNguoiNhan.Contains(query)) ||
                             (d.Sdt != null && d.Sdt.Contains(query))))
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaSanPhamNavigation)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new
                {
                    Id = d.MaDonHang,
                    Date = d.NgayDat != null ? d.NgayDat.Value.ToString("yyyy-MM-dd") : "",
                    Status = d.TrangThaiDonHang == TrangThaiDonHang.ChuaXacNhan ? "pending" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangXuLy ? "processing" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangGiaoHang ? "shipping" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "completed" : "canceled",
                    Total = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    FinalAmount = d.FinalAmount,
                    TenNguoiNhan = d.TenNguoiNhan,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    LyDoHuy = d.LyDoHuy,
                    Sdt = d.Sdt
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng nào khớp với tiêu chí tìm kiếm." });
            }

            return Ok(orders);
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

                // Kiểm tra nếu tài khoản đang bị khóa
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
                    // Cập nhật trạng thái đơn hàng
                    order.TrangThaiDonHang = Data.TrangThaiDonHang.DaHuy;
                    order.LyDoHuy = request.LyDoHuy.Trim();

                    // Tăng số lần hủy đơn hàng
                    nguoiDung.CancelConunt = (nguoiDung.CancelConunt ?? 0) + 1;

                    // Kiểm tra nếu đã hủy >= 3 lần thì khóa tài khoản
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

                    // SỬA: Luôn trả về thông báo thành công
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
            // Thử lấy từ các claim types khác nhau
            var claims = HttpContext.User?.Claims?.ToList();
            if (claims == null || !claims.Any()) return null;

            // Ưu tiên lấy MaNguoiDung trước
            var userId = claims.FirstOrDefault(c => c.Type == "MaNguoiDung")?.Value;

            // Nếu không có thì thử các claim khác
            if (string.IsNullOrEmpty(userId))
            {
                userId = claims.FirstOrDefault(c => c.Value.StartsWith("ND") || c.Value.StartsWith("KH") || c.Value.StartsWith("AD"))?.Value;
            }

            // Fallback về các claim standard
            if (string.IsNullOrEmpty(userId))
            {
                userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
                         claims.FirstOrDefault(c => c.Type == "nameid")?.Value ??
                         claims.FirstOrDefault(c => c.Type == "sub")?.Value ??
                         claims.FirstOrDefault(c => c.Type == "userId")?.Value ??
                         claims.FirstOrDefault(c => c.Type == "id")?.Value;
            }

            // Nếu vẫn không có, thử lấy từ Identity.Name
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
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
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
                chiTietDonHangs = donHang.ChiTietDonHangs.Select(ct => new
                {
                    soLuong = ct.SoLuong,
                    gia = ct.Gia,
                    thanhTien = ct.ThanhTien,
                    maSanPham = ct.MaSanPham,
                    mauSac = ExtractColorFromProductCode(ct.MaSanPham),
                    kichThuoc = ExtractSizeFromProductCode(ct.MaSanPham),
                    mauSacHex = ExtractColorHexFromProductCode(ct.MaSanPham),
                    maSanPhamNavigation = ct.MaSanPhamNavigation != null ? new
                    {
                        tenSanPham = ct.MaSanPhamNavigation.TenSanPham,
                        hinhAnhs = ct.MaSanPhamNavigation.HinhAnhs.Select(h => new {
                            link = !string.IsNullOrEmpty(h.Link) ? h.Link :
                        (h.Data != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(h.Data)}" : null)
                        }).ToList()
                    } : null
                }).ToList()
            });
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

        private string ConvertHexToColorName(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return null;

            // Dictionary để map hex color thành tên màu tiếng Việt
            var colorMap = new Dictionary<string, string>
            {
                { "ff0000", "Đỏ" },
                { "FF0000", "Đỏ" },
                { "00ff00", "Xanh lá" },
                { "00FF00", "Xanh lá" },
                { "0000ff", "Xanh dương" },
                { "0000FF", "Xanh dương" },
                { "ffff00", "Vàng" },
                { "FFFF00", "Vàng" },
                { "ff00ff", "Hồng" },
                { "FF00FF", "Hồng" },
                { "00ffff", "Xanh cyan" },
                { "00FFFF", "Xanh cyan" },
                { "000000", "Đen" },
                { "ffffff", "Trắng" },
                { "FFFFFF", "Trắng" },
                { "808080", "Xám" },
                { "ffa500", "Cam" },
                { "FFA500", "Cam" },
                { "800080", "Tím" },
                { "a52a2a", "Nâu" },
                { "A52A2A", "Nâu" }
            };

            return colorMap.ContainsKey(hex) ? colorMap[hex] : hex;
        }

        private string GetImageFromSanPham(string? maSanPham)
        {
            if (string.IsNullOrEmpty(maSanPham)) return "/placeholder.svg";

            var image = _context.HinhAnhs.FirstOrDefault(h => maSanPham.Contains(h.MaSanPham));
            if (image == null) return "/placeholder.svg";

            return !string.IsNullOrEmpty(image.Link)
                ? image.Data != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(image.Data)}"
                    : "/placeholder.svg"
                : image.Link;
        }

        private string GetImageFromCombo(int? maCombo)
        {
            if (maCombo == null) return "/placeholder.svg";

            var chiTiet = _context.ChiTietComBos
                .Include(ct => ct.MaSanPhamNavigation)
                .ThenInclude(sp => sp.HinhAnhs)
                .FirstOrDefault(ct => ct.MaComBo == maCombo);

            if (chiTiet?.MaSanPhamNavigation?.HinhAnhs == null || !chiTiet.MaSanPhamNavigation.HinhAnhs.Any())
                return "/placeholder.svg";

            var image = chiTiet.MaSanPhamNavigation.HinhAnhs.FirstOrDefault();
            return !string.IsNullOrEmpty(image?.Link)
                ? image.Link
                : image?.Data != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(image.Data)}"
                    : "/placeholder.svg";
        }
        public class CancelOrderRequest
        {
            [Required(ErrorMessage = "Lý do hủy không được để trống")]
            [StringLength(500, ErrorMessage = "Lý do hủy không được quá 500 ký tự")]
            public string LyDoHuy { get; set; }
        }


    }
}