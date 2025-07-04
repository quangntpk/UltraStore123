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
                .ThenInclude(sp => sp.HinhAnhs)
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(cd => cd.MaComboNavigation)
                .ThenInclude(c => c.ChiTietComBos)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .ThenInclude(sp => sp.HinhAnhs)
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
                    Items = d.ChiTietDonHangs.Select(cd => new
                    {
                        Id = cd.MaCtdh,
                        Name = cd.MaCombo != null
                            ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                            : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                        Quantity = cd.SoLuong,
                        Price = cd.Gia,
                        Color = cd.MaCombo == null ? ExtractColorFromProductCode(cd.MaSanPham) : null,
                        Size = cd.MaCombo == null ? ExtractSizeFromProductCode(cd.MaSanPham) : null,
                        ProductCode = cd.MaSanPham,
                        ComboCode = cd.MaCombo,
                        Image = cd.MaCombo != null
                            ? GetImageFromCombo(cd.MaCombo)
                            : GetImageFromSanPham(cd.MaSanPham)
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
                    Id = d.MaDonHang,
                    Date = d.NgayDat != null ? d.NgayDat.Value.ToString("yyyy-MM-dd") : "",
                    Status = d.TrangThaiDonHang == TrangThaiDonHang.ChuaXacNhan ? "pending" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangXuLy ? "processing" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DangGiaoHang ? "shipping" :
                             d.TrangThaiDonHang == TrangThaiDonHang.DaGiaoHang ? "completed" : "canceled",
                    Total = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    FinalAmount = d.FinalAmount,
                    DiscountAmount = d.DiscountAmount,
                    ShippingFee = d.ShippingFee,
                    Items = d.ChiTietDonHangs.Select(cd => new
                    {
                        Id = cd.MaCtdh,
                        Name = cd.MaCombo != null
                            ? cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại"
                            : cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại",
                        Quantity = cd.SoLuong,
                        Price = cd.Gia,
                        Color = cd.MaCombo == null ? ExtractColorFromProductCode(cd.MaSanPham) : null,
                        Size = cd.MaCombo == null ? ExtractSizeFromProductCode(cd.MaSanPham) : null,
                        ColorHex = cd.MaCombo == null ? ExtractColorHexFromProductCode(cd.MaSanPham) : null,
                        ProductCode = cd.MaSanPham,
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
            string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value;
            if (string.IsNullOrEmpty(maNguoiDung))
            {
                return Unauthorized(new { message = "Không xác định được người dùng từ token." });
            }

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
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin người dùng" });
            }

            // Kiểm tra xem tài khoản có bị khóa không
            if (user.LockoutEndDate != null && user.LockoutEndDate > DateTime.Now)
            {
                return BadRequest(new
                {
                    message = $"Tài khoản của bạn bị khóa đến {user.LockoutEndDate.Value.ToString("dd/MM/yyyy HH:mm:ss")}",
                    isAccountLocked = true,
                    lockoutMessage = $"Tài khoản của bạn bị khóa đến {user.LockoutEndDate.Value.ToString("dd/MM/yyyy HH:mm:ss")}"
                });
            }

            if (string.IsNullOrEmpty(lyDoHuy))
            {
                return BadRequest(new { message = "Lý do hủy không được để trống" });
            }

            // Cập nhật đơn hàng
            order.TrangThaiDonHang = TrangThaiDonHang.DaHuy;
            order.LyDoHuy = lyDoHuy;

            // Tăng số lần hủy
            user.CancelConunt = (user.CancelConunt ?? 0) + 1;

            // Khóa tài khoản nếu hủy quá 3 lần
            bool willBeLocked = user.CancelConunt > 3;
            if (willBeLocked)
            {
                user.LockoutEndDate = DateTime.Now.AddDays(3);
                user.TrangThai = 1; // Bị khóa
            }

            try
            {
                await _context.SaveChangesAsync();

                if (willBeLocked)
                {
                    return Ok(new
                    {
                        message = "Đơn hàng đã được hủy. Tài khoản của bạn đã bị khóa do hủy đơn hàng quá 3 lần.",
                        isAccountLocked = true,
                        lockoutMessage = $"Tài khoản của bạn đã bị khóa đến {user.LockoutEndDate.Value.ToString("dd/MM/yyyy HH:mm:ss")}"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Hủy đơn thành công",
                        isAccountLocked = false,
                        cancelCount = user.CancelConunt
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi hủy đơn hàng", error = ex.Message });
            }
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
    }
}