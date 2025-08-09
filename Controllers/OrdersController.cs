using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using System.Security.Claims;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ✅ CHỈ YÊU CẦU LOGIN
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ FIXED: Đọc claims đúng định dạng từ JWT
        private async Task<(string userId, int role, NguoiDung user)> GetCurrentUserInfo()
        {
            string userId = null;
            string roleFromClaims = null;

            Console.WriteLine($"[DEBUG] ===== USER INFO DEBUG =====");
            Console.WriteLine($"[DEBUG] All user claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"[DEBUG] {claim.Type}: {claim.Value}");
            }

            // ✅ Method 1: Tìm từ các claim types thường gặp
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"[DEBUG] NameIdentifier: {userId}");

            // ✅ Method 2: Tìm từ custom claims
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst("maNguoiDung")?.Value;
                Console.WriteLine($"[DEBUG] maNguoiDung claim: {userId}");
            }

            // ✅ Method 3: Tìm từ nameid, sub, userId, id
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst("nameid")?.Value ??
                         User.FindFirst("sub")?.Value ??
                         User.FindFirst("userId")?.Value ??
                         User.FindFirst("id")?.Value;
                Console.WriteLine($"[DEBUG] Standard claims: {userId}");
            }

            // ✅ Method 4: Tìm từ email nếu không có userId
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst(ClaimTypes.Email)?.Value ??
                         User.FindFirst("email")?.Value;
                Console.WriteLine($"[DEBUG] Email claim: {userId}");
            }

            // ✅ Method 5: Duyệt tất cả claims tìm pattern AD, ND, KH
            if (string.IsNullOrEmpty(userId))
            {
                foreach (var claim in User.Claims)
                {
                    if (!string.IsNullOrEmpty(claim.Value) &&
                        (claim.Value.StartsWith("AD") || claim.Value.StartsWith("ND") || claim.Value.StartsWith("KH")))
                    {
                        userId = claim.Value;
                        Console.WriteLine($"[DEBUG] Found user ID pattern from claim {claim.Type}: {userId}");
                        break;
                    }
                }
            }

            // ✅ ROLE EXTRACTION: Tìm vai trò từ claims
            roleFromClaims = User.FindFirst(ClaimTypes.Role)?.Value ??
                      User.FindFirst("role")?.Value ??
                      User.FindFirst("vaiTro")?.Value;
            Console.WriteLine($"[DEBUG] Role from claims: {roleFromClaims}");

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng từ token");
            }

            Console.WriteLine($"[DEBUG] Final userId: {userId}");

            // ✅ TÌM USER TRONG DATABASE: Hỗ trợ nhiều cách tìm
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u =>
                u.MaNguoiDung == userId ||
                u.Email == userId ||
                u.TaiKhoan == userId);

            if (user == null)
            {
                Console.WriteLine($"[DEBUG] User not found with ID: {userId}, trying email lookup...");

                // Thử tìm bằng email từ claims
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
                    if (user != null)
                    {
                        userId = user.MaNguoiDung; // Update userId to the correct one
                        Console.WriteLine($"[DEBUG] Found user by email: {user.MaNguoiDung}");
                    }
                }
            }

            if (user == null)
            {
                throw new UnauthorizedAccessException($"Người dùng {userId} không tồn tại trong hệ thống");
            }

            // ✅ ROLE DETERMINATION: Ưu tiên role từ database
            int finalRole = user.VaiTro ?? 0;

            // Fallback to role from token if database role is null or 0
            if (finalRole == 0 && !string.IsNullOrEmpty(roleFromClaims))
            {
                if (int.TryParse(roleFromClaims, out int tokenRole))
                {
                    finalRole = tokenRole;
                    Console.WriteLine($"[DEBUG] Using role from token: {finalRole}");
                }
            }

            Console.WriteLine($"[DEBUG] Found user: {user.MaNguoiDung}, Email: {user.Email}, DB Role: {user.VaiTro}, Final Role: {finalRole}");
            Console.WriteLine($"[DEBUG] ===== END USER INFO DEBUG =====");

            return (user.MaNguoiDung, finalRole, user);
        }

        // ✅ AUTHORIZATION CHECK: Kiểm tra quyền Admin/Staff
        private async Task<bool> IsAuthorizedForOrders()
        {
            try
            {
                var (_, currentUserRole, _) = await GetCurrentUserInfo();
                bool authorized = currentUserRole == 1 || currentUserRole == 2; // Admin hoặc Staff
                Console.WriteLine($"[DEBUG] Authorization check: Role={currentUserRole}, Authorized={authorized}");
                return authorized;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Authorization failed: {ex.Message}");
                return false;
            }
        }

        // ✅ GET ORDERS: Fixed authorization
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                Console.WriteLine("[DEBUG] Starting GetOrders request");

                // ✅ KIỂM TRA QUYỀN TRUY CẬP
                if (!await IsAuthorizedForOrders())
                {
                    // Log thêm thông tin để debug
                    try
                    {
                        var (userId, currentUserRole, user) = await GetCurrentUserInfo();
                        Console.WriteLine($"[DEBUG] Access denied for user {userId} with role {currentUserRole}");
                        return Forbid($"Bạn không có quyền truy cập chức năng này. Vai trò hiện tại: {currentUserRole}. Chỉ Admin (1) và Nhân viên (2) mới được phép.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Could not get user info for deny message: {ex.Message}");
                        return Forbid("Bạn không có quyền truy cập chức năng này. Chỉ Admin và Nhân viên mới được phép.");
                    }
                }

                // Lấy thông tin user
                var (currentUserId, userRole, currentUser) = await GetCurrentUserInfo();
                Console.WriteLine($"[DEBUG] User {currentUserId} with role {userRole} accessing orders");

                // Load all products and supports data once
                var allSanPhams = await _context.SanPhams
                    .Include(sp => sp.HinhAnhs)
                    .AsNoTracking()
                    .ToListAsync();

                var allSupports = await _context.DonHangSupports
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[DEBUG] Loaded {allSanPhams.Count} products and {allSupports.Count} supports");

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
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();

                Console.WriteLine($"[DEBUG] Loaded {orders.Count} orders");

                var result = orders.Select(d => new
                {
                    MaDonHang = d.MaDonHang,
                    TenNguoiNhan = d.TenNguoiNhan,
                    NgayDat = d.NgayDat?.ToString("dd/MM/yyyy") ?? "",
                    TrangThaiDonHang = (int)d.TrangThaiDonHang,
                    TrangThaiThanhToan = (int)d.TrangThaiHang,
                    HinhThucThanhToan = d.TrangThaiHang == TrangThaiThanhToan.ThanhToanKhiNhanHang ? "COD" : "VNPay",
                    LyDoHuy = d.LyDoHuy,
                    TongTien = d.ChiTietDonHangs.Sum(cd => cd.ThanhTien ?? 0),
                    FinalAmount = d.FinalAmount,
                    DiscountAmount = d.DiscountAmount,
                    ShippingFee = d.ShippingFee,
                    DiaChi = d.DiaChi,
                    SDT = d.Sdt,

                    TenSanPhamHoacCombo = d.ChiTietDonHangs.Select(cd => cd.MaCombo != null
                        ? (cd.MaComboNavigation?.TenComBo ?? "Combo không tồn tại")
                        : (cd.MaSanPhamNavigation?.TenSanPham ?? "Sản phẩm không tồn tại"))
                        .FirstOrDefault(),

                    MaNhanVien = d.MaNhanVien,
                    HoTenNhanVien = d.MaNhanVienNavigation?.HoTen,
                    MaNguoiDung = d.MaNguoiDung,
                    HoTenKhachHang = d.MaNguoiDungNavigation?.HoTen,

                    // ✅ THÊM: Thông tin để Frontend kiểm tra quyền
                    CanProcess = userRole == 1 || string.IsNullOrEmpty(d.MaNhanVien) || d.MaNhanVien == currentUserId,

                    ChiTietSanPhams = d.ChiTietDonHangs.Select(cd => new
                    {
                        MaChiTietDh = cd.MaCtdh,
                        LaCombo = cd.MaCombo != null,
                        TenSanPham = cd.MaCombo != null
                            ? (cd.MaComboNavigation?.TenComBo ?? "Combo không tồn tại")
                            : (cd.MaSanPhamNavigation?.TenSanPham ?? "Sản phẩm không tồn tại"),
                        SoLuong = cd.SoLuong,
                        Gia = cd.Gia,
                        ThanhTien = cd.ThanhTien,
                        MaCombo = cd.MaCombo,
                        MaSanPham = cd.MaSanPham,

                        MauSac = cd.MaCombo == null ? ExtractColorFromProductCode(cd.MaSanPham) : null,
                        KichThuoc = cd.MaCombo == null ? ExtractSizeFromProductCode(cd.MaSanPham) : null,

                        HinhAnh = cd.MaCombo != null
                            ? GetComboImage(cd.MaComboNavigation, allSanPhams)
                            : GetImageByProductId_Enhanced(cd.MaSanPham, allSanPhams),

                        Combo = cd.MaCombo != null && cd.MaComboNavigation != null ? new
                        {
                            TenCombo = cd.MaComboNavigation.TenComBo,
                            GiaCombo = cd.MaComboNavigation.TongGia,
                            SanPhamsTrongCombo = cd.MaComboNavigation.ChiTietComBos.Select(ct =>
                            {
                                var actualProductCode = GetActualProductFromSupports(cd.MaCtdh, ct.MaChiTietComBo, allSupports) ?? ct.MaSanPham;

                                return new
                                {
                                    TenSanPham = GetProductNameByCode_Fixed(ct.MaSanPham, allSanPhams),
                                    SoLuong = ct.SoLuong,
                                    Gia = GetProductPriceByCode_Fixed(ct.MaSanPham, allSanPhams),
                                    ThanhTien = GetProductPriceByCode_Fixed(ct.MaSanPham, allSanPhams) * (ct.SoLuong ?? 0),
                                    MaSanPham = actualProductCode,
                                    MauSac = ExtractColorFromProductCode(actualProductCode),
                                    KichThuoc = ExtractSizeFromProductCode(actualProductCode),
                                    HinhAnh = GetImageByProductId_Enhanced(ct.MaSanPham, allSanPhams)
                                };
                            }).ToList()
                        } : null
                    }).ToList()
                }).ToList();

                Console.WriteLine($"[DEBUG] Returning {result.Count} processed orders");
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[ERROR] Unauthorized: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception in GetOrders: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // ✅ APPROVE ORDER
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveOrder(int id, [FromBody] ApproveOrderRequest request)
        {
            try
            {
                if (!await IsAuthorizedForOrders())
                {
                    return Forbid("Bạn không có quyền duyệt đơn hàng. Chỉ Admin và Nhân viên mới được phép.");
                }

                var (currentUserId, userRole, currentUser) = await GetCurrentUserInfo();
                Console.WriteLine($"[DEBUG] User {currentUserId} (role {userRole}) attempting to approve order {id}");

                var order = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(cd => cd.MaComboNavigation)
                    .FirstOrDefaultAsync(d => d.MaDonHang == id);

                if (order == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại" });

                if (order.TrangThaiDonHang != Data.TrangThaiDonHang.ChuaXacNhan &&
                    order.TrangThaiDonHang != Data.TrangThaiDonHang.DangXuLy &&
                    order.TrangThaiDonHang != Data.TrangThaiDonHang.DangGiaoHang)
                {
                    return BadRequest(new { message = "Không thể duyệt đơn hàng ở trạng thái này" });
                }

                if (string.IsNullOrEmpty(order.MaNhanVien))
                {
                    order.MaNhanVien = currentUserId;
                    Console.WriteLine($"[DEBUG] Assigning order {id} to user {currentUserId}");
                }
                else
                {
                    if (userRole != 1)
                    {
                        if (order.MaNhanVien != currentUserId)
                        {
                            return Forbid("Đơn hàng đã được gán cho nhân viên khác xử lý.");
                        }
                    }
                    Console.WriteLine($"[DEBUG] User {currentUserId} has permission to approve order {id}");
                }

                order.TrangThaiDonHang = (Data.TrangThaiDonHang)((int)order.TrangThaiDonHang + 1);

                if (order.TrangThaiDonHang == Data.TrangThaiDonHang.DaGiaoHang)
                {
                    order.TrangThaiHang = Data.TrangThaiThanhToan.ThanhToanVNPay;
                }

                _context.DonHangs.Update(order);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] Order {id} approved successfully by user {currentUserId}");
                return Ok(new { message = "Duyệt đơn thành công", assignedStaff = currentUserId });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error approving order {id}: {ex.Message}");
                return BadRequest(new { message = $"Lỗi khi lưu dữ liệu: {ex.Message}" });
            }
        }

        // ✅ CANCEL ORDER
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string lyDoHuy)
        {
            try
            {
                if (!await IsAuthorizedForOrders())
                {
                    return Forbid("Bạn không có quyền hủy đơn hàng. Chỉ Admin và Nhân viên mới được phép.");
                }

                var (currentUserId, userRole, currentUser) = await GetCurrentUserInfo();
                Console.WriteLine($"[DEBUG] User {currentUserId} (role {userRole}) attempting to cancel order {id}");

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

                if (userRole != 1)
                {
                    if (!string.IsNullOrEmpty(order.MaNhanVien) && order.MaNhanVien != currentUserId)
                    {
                        return Forbid("Bạn không có quyền hủy đơn hàng này. Chỉ nhân viên đã duyệt đơn hoặc admin mới có thể hủy.");
                    }
                }

                if (string.IsNullOrEmpty(order.MaNhanVien))
                {
                    order.MaNhanVien = currentUserId;
                }

                order.TrangThaiDonHang = Data.TrangThaiDonHang.DaHuy;
                order.LyDoHuy = lyDoHuy;

                _context.DonHangs.Update(order);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] Order {id} cancelled successfully by user {currentUserId}");
                return Ok(new { message = "Hủy đơn thành công" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error cancelling order {id}: {ex.Message}");
                return BadRequest(new { message = $"Lỗi khi hủy đơn: {ex.Message}" });
            }
        }

        // ✅ GET CANCELLED ORDERS
        [HttpGet("cancelled")]
        public async Task<IActionResult> GetCancelledOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (!await IsAuthorizedForOrders())
                {
                    return Forbid("Bạn không có quyền xem đơn hàng đã hủy. Chỉ Admin và Nhân viên mới được phép.");
                }

                var (currentUserId, userRole, currentUser) = await GetCurrentUserInfo();
                Console.WriteLine($"[DEBUG] User {currentUserId} (role {userRole}) accessing cancelled orders");

                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 10;

                var allSanPhams = await _context.SanPhams
                    .Include(sp => sp.HinhAnhs)
                    .AsNoTracking()
                    .ToListAsync();

                var allSupports = await _context.DonHangSupports
                    .AsNoTracking()
                    .ToListAsync();

                var totalCancelledOrders = await _context.DonHangs
                    .Where(d => d.TrangThaiDonHang == Data.TrangThaiDonHang.DaHuy)
                    .CountAsync();

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
                    DiscountAmount = d.DiscountAmount,
                    ShippingFee = d.ShippingFee,

                    TenSanPhamHoacCombo = d.ChiTietDonHangs.Select(cd => cd.MaCombo != null
                        ? (cd.MaComboNavigation != null ? cd.MaComboNavigation.TenComBo : "Combo không tồn tại")
                        : (cd.MaSanPhamNavigation != null ? cd.MaSanPhamNavigation.TenSanPham : "Sản phẩm không tồn tại"))
                        .FirstOrDefault(),

                    TongSoSanPham = d.ChiTietDonHangs.Sum(cd => cd.SoLuong ?? 0),

                    MaNhanVien = d.MaNhanVien,
                    HoTenNhanVien = d.MaNhanVienNavigation != null ? d.MaNhanVienNavigation.HoTen : "Chưa có nhân viên xử lý",

                    MaNguoiDung = d.MaNguoiDung,
                    HoTenKhachHang = d.MaNguoiDungNavigation != null ? d.MaNguoiDungNavigation.HoTen : "Khách hàng không tồn tại",

                    DiaChi = d.DiaChi,
                    SoDienThoai = d.Sdt,

                    CanProcess = userRole == 1 || string.IsNullOrEmpty(d.MaNhanVien) || d.MaNhanVien == currentUserId,

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

                        MauSac = cd.MaCombo == null && !string.IsNullOrEmpty(cd.MaSanPham) ? ParseColorFromProductId(cd.MaSanPham) : null,
                        KichThuoc = cd.MaCombo == null && !string.IsNullOrEmpty(cd.MaSanPham) ? ParseSizeFromProductId(cd.MaSanPham) : null,

                        HinhAnh = cd.MaCombo != null
                            ? (cd.MaComboNavigation != null && cd.MaComboNavigation.ChiTietComBos.Any()
                                ? GetImageByProductId(cd.MaComboNavigation.ChiTietComBos.FirstOrDefault().MaSanPham, allSanPhams)
                                : null)
                            : GetImageByProductId(cd.MaSanPham, allSanPhams),

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
                                MaSanPham = GetActualProductFromComboOrderEnhanced(cd.MaCtdh, ct.MaChiTietComBo, ct.MaSanPham),
                                MauSac = ParseColorFromProductId(GetActualProductFromComboOrderEnhanced(cd.MaCtdh, ct.MaChiTietComBo, ct.MaSanPham)),
                                KichThuoc = ParseSizeFromProductId(GetActualProductFromComboOrderEnhanced(cd.MaCtdh, ct.MaChiTietComBo, ct.MaSanPham)),
                                HinhAnh = GetImageByProductId(ct.MaSanPham, allSanPhams)
                            }).ToList()
                        } : null
                    }).ToList()
                }).ToList();

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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error getting cancelled orders: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // ✅ GET ORDERS BY USER
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdersByUserId(string id)
        {
            try
            {
                var (currentUserId, userRole, currentUser) = await GetCurrentUserInfo();

                // Kiểm tra quyền: Admin/Staff xem được tất cả, User chỉ xem được của mình
                if (userRole == 0 && currentUserId != id) // User role = 0
                {
                    return Forbid("Bạn chỉ có thể xem đơn hàng của chính mình");
                }

                if (string.IsNullOrEmpty(id) || id == "undefined")
                {
                    return BadRequest(new { message = "ID người dùng không hợp lệ." });
                }

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
                            MauSac = cd.MaCombo == null && !string.IsNullOrEmpty(cd.MaSanPham) ? ParseColorFromProductId(cd.MaSanPham) : null,
                            KichThuoc = cd.MaCombo == null && !string.IsNullOrEmpty(cd.MaSanPham) ? ParseSizeFromProductId(cd.MaSanPham) : null,
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
                                    MaSanPham = GetActualProductFromComboOrderEnhanced(cd.MaCtdh, ct.MaChiTietComBo, ct.MaSanPham),
                                    MauSac = ParseColorFromProductId(GetActualProductFromComboOrderEnhanced(cd.MaCtdh, ct.MaChiTietComBo, ct.MaSanPham)),
                                    KichThuoc = ParseSizeFromProductId(GetActualProductFromComboOrderEnhanced(cd.MaCtdh, ct.MaChiTietComBo, ct.MaSanPham)),
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error getting orders for user {id}: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // ✅ HELPER METHODS - Giữ nguyên
        private string GetActualProductFromSupports(int chiTietDonHangId, int maChiTietCombo, List<DonHangSupport> allSupports)
        {
            try
            {
                var exactMatch = allSupports.FirstOrDefault(s =>
                    s.ChiTietGioHang == chiTietDonHangId && s.MaChiTietCombo == maChiTietCombo);

                if (exactMatch != null)
                {
                    return exactMatch.MaSanPham;
                }

                var latestByCombo = allSupports
                    .Where(s => s.MaChiTietCombo == maChiTietCombo && s.ChiTietGioHang == 0)
                    .OrderByDescending(s => s.ID)
                    .FirstOrDefault();

                if (latestByCombo != null)
                {
                    return latestByCombo.MaSanPham;
                }

                var fallback = allSupports.FirstOrDefault(s => s.ChiTietGioHang == chiTietDonHangId);
                if (fallback != null)
                {
                    return fallback.MaSanPham;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error getting actual product from supports: {ex.Message}");
                return null;
            }
        }

        private string GetComboImage(ComBoSanPham combo, List<SanPham> allSanPhams)
        {
            if (combo?.ChiTietComBos?.Any() != true) return null;

            var firstProduct = combo.ChiTietComBos.FirstOrDefault();
            return firstProduct != null ? GetImageByProductId_Enhanced(firstProduct.MaSanPham, allSanPhams) : null;
        }

        private string GetImageByProductId_Enhanced(string productId, List<SanPham> allSanPhams)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            try
            {
                var exactMatch = _context.HinhAnhs
                    .Where(ha => ha.MaSanPham == productId)
                    .Select(ha => ha.Link)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(exactMatch))
                    return exactMatch;

                var baseProductId = productId.Contains("_") ? productId.Split('_')[0] : productId;

                var baseMatch = _context.HinhAnhs
                    .Where(ha => ha.MaSanPham == baseProductId || ha.MaSanPham.StartsWith(baseProductId + "_"))
                    .Select(ha => ha.Link)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(baseMatch))
                    return baseMatch;

                var productWithImage = allSanPhams?.FirstOrDefault(sp =>
                    sp.MaSanPham == productId ||
                    sp.MaSanPham == baseProductId ||
                    (sp.MaSanPham != null && sp.MaSanPham.StartsWith(baseProductId + "_")));

                return productWithImage?.HinhAnhs?.FirstOrDefault()?.Link;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error getting image for product {productId}: {ex.Message}");
                return null;
            }
        }

        private string GetProductNameByCode_Fixed(string productCode, List<SanPham> allSanPhams)
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

        private decimal GetProductPriceByCode_Fixed(string productCode, List<SanPham> allSanPhams)
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

        private string ExtractColorFromProductCode(string productCode)
        {
            if (string.IsNullOrEmpty(productCode)) return null;

            var parts = productCode.Split('_');
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

        private string ExtractSizeFromProductCode(string productCode)
        {
            if (string.IsNullOrEmpty(productCode)) return null;

            var parts = productCode.Split('_');
            if (parts.Length >= 3)
            {
                var sizeCode = parts[2];
                var sizeMap = new Dictionary<string, string>
                {
                    {"S", "S"}, {"M", "M"}, {"L", "L"},
                    {"XL", "XL"}, {"XXL", "XXL"}, {"XXXL", "XXXL"}
                };
                return sizeMap.ContainsKey(sizeCode) ? sizeMap[sizeCode] : sizeCode;
            }
            return null;
        }

        private string GetActualProductFromComboOrderEnhanced(int chiTietDonHangId, int maChiTietCombo, string baseProductCode)
        {
            try
            {
                var allSupports = _context.DonHangSupports.ToList();
                return GetActualProductFromSupports(chiTietDonHangId, maChiTietCombo, allSupports) ?? baseProductCode;
            }
            catch
            {
                return baseProductCode;
            }
        }

        private string ParseColorFromProductId(string productId)
        {
            return ExtractColorFromProductCode(productId);
        }

        private string ParseSizeFromProductId(string productId)
        {
            return ExtractSizeFromProductCode(productId);
        }

        private string GetImageByProductId(string productId, List<SanPham> allSanPhams = null)
        {
            return GetImageByProductId_Enhanced(productId, allSanPhams);
        }

        private string GetProductNameByCode(string productCode, List<SanPham> allSanPhams)
        {
            return GetProductNameByCode_Fixed(productCode, allSanPhams);
        }

        private decimal GetProductPriceByCode(string productCode, List<SanPham> allSanPhams)
        {
            return GetProductPriceByCode_Fixed(productCode, allSanPhams);
        }


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
            DaHuy = 4
        }

        // Thêm endpoint này vào OrdersController

        // GET CURRENT USER INFO - API để Frontend lấy thông tin user hiện tại
        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var (currentUserId, userRole, currentUser) = await GetCurrentUserInfo();

                return Ok(new
                {
                    maNguoiDung = currentUserId,
                    fullName = currentUser.HoTen,
                    email = currentUser.Email,
                    vaiTro = userRole,
                    isAdmin = userRole == 1,
                    isStaff = userRole == 2
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error getting current user: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}