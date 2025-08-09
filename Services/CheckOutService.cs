using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UltraStrore.Data;
using UltraStrore.Data.Temp;
using UltraStrore.Helper;
using UltraStrore.Models.DTO;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class CheckOutService : ICheckOutServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckOutService> _logger;
        private readonly VnPayConfig _vnpayConfig;
        private readonly IVnPayServies _vnPayService;
        public static bool InstantBuy = false;
        public static DonHang donHangTemp;
        public static List<ChiTietDonHang> chiTietDonHangTemp;
        public static List<DonHangSupport> donHangSupportsTemp;
        public CheckOutService(ApplicationDbContext context, ILogger<CheckOutService> logger, VnPayConfig vnpayConfig, IVnPayServies vnPayService)
        {
            _context = context;
            _logger = logger;
            _vnpayConfig = vnpayConfig;
            _vnPayService = vnPayService;
        }
        public async Task<PaymentResponse> InstantCheckout(PaymentRequestDto1 request, HttpContext httpContext)
        {
            InstantBuy = true;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (request == null || request.CartId <= 0)
                {
                    return new PaymentResponse { Success = false, Message = "Yêu cầu thanh toán không hợp lệ" };
                }
                if (string.IsNullOrEmpty(request.PaymentMethod) || !new[] { "cash", "cod", "vnpay" }.Contains(request.PaymentMethod.ToLower()))
                {
                    return new PaymentResponse { Success = false, Message = "Phương thức thanh toán không hợp lệ" };
                }
                if (request.FinalAmount <= 0)
                {
                    return new PaymentResponse { Success = false, Message = "Số tiền cuối cùng không hợp lệ" };
                }

                _logger.LogInformation($"Processing payment: CartId={request.CartId}, PaymentMethod={request.PaymentMethod}, FinalAmount={request.FinalAmount}");

                var cart = await _context.GioHangs
                    .Include(c => c.ChiTietGioHangs)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                    .Include(c => c.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(c => c.MaNguoiDung == request.UserId);

                if (cart == null)
                {
                    cart = new GioHang
                    {
                        MaNguoiDung = request.UserId,
                    };
                    _context.GioHangs.Add(cart);
                    _context.SaveChangesAsync();
                }
                decimal originalAmount = request.FinalAmount;
                decimal discountAmount = request.DiscountAmount;
                decimal shippingFee = request.ShippingFee;
                decimal finalAmount = request.FinalAmount;

                var orderDto = new OrderDto
                {
                    MaNguoiDung = cart.MaNguoiDung,
                    TenNguoiNhan = request.TenNguoiNhan ?? cart.MaNguoiDungNavigation?.HoTen,
                    Sdt = request.Sdt,
                    DiaChi = request.DiaChi,
                    NgayDat = DateTime.Now,
                    TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan,
                    TrangThaiHang = request.PaymentMethod.ToLower() switch
                    {
                        "cash" => TrangThaiThanhToan.ThanhToanTienMat,
                        "cod" => TrangThaiThanhToan.ThanhToanKhiNhanHang,
                        _ => TrangThaiThanhToan.ThanhToanVNPay
                    },
                    ChiTietDonHangs = new List<ChiTietDonHang>(),
                    DiscountAmount = discountAmount,
                    ShippingFee = shippingFee,
                    FinalAmount = finalAmount
                };

                if (string.IsNullOrEmpty(orderDto.TenNguoiNhan) || string.IsNullOrEmpty(orderDto.Sdt) || string.IsNullOrEmpty(orderDto.DiaChi))
                {
                    return new PaymentResponse { Success = false, Message = "Thông tin người nhận không hợp lệ" };
                }

                var chiTietDonHangs = new List<ChiTietDonHang>();
                var donHangSupports = new List<DonHangSupport>();

                foreach (var item in request.items)
                {
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        MaSanPham = item.IdSanPham + "_" + item.MauSac + "_" + item.KickThuoc,
                        SoLuong = item.SoLuongMua,
                        Gia = item.TienSanPham,
                        ThanhTien = item.TienSanPham * item.SoLuongMua,
                        MaCombo = null,
                        SanPhamMaSanPham = item.IdSanPham,
                    };
                    chiTietDonHangs.Add(chiTietDonHang);
                    orderDto.ChiTietDonHangs.Add(chiTietDonHang);

                    //var chiTietGioHangSupports = _context.GioHangSupports
                    //    .Where(g => g.ChiTietGioHang == item.MaCtgh && item.MaCombo != null)
                    //    .ToList();

                    //foreach (var k in chiTietGioHangSupports)
                    //{
                    //    var donHangSupport = new DonHangSupport
                    //    {
                    //        MaSanPham = k.MaSanPham,
                    //        ChiTietGioHang = chiTietDonHang.MaCtdh,
                    //        MaChiTietCombo = k.MaChiTietCombo,
                    //        SoLuong = k.SoLuong,
                    //    };
                    //    donHangSupports.Add(donHangSupport);
                    //}
                }

                if (request.PaymentMethod.ToLower() == "cash")
                {

                    var donHang = new DonHang
                    {
                        MaNguoiDung = orderDto.MaNguoiDung,
                        TenNguoiNhan = orderDto.TenNguoiNhan,
                        Sdt = orderDto.Sdt,
                        DiaChi = orderDto.DiaChi,
                        NgayDat = orderDto.NgayDat,
                        TrangThaiDonHang = TrangThaiDonHang.DaGiaoHang,
                        TrangThaiHang = orderDto.TrangThaiHang,
                        DiscountAmount = orderDto.DiscountAmount,
                        ShippingFee = orderDto.ShippingFee,
                        FinalAmount = orderDto.FinalAmount
                    };
                    _context.Add(donHang);
                    await _context.SaveChangesAsync();

                    foreach (var chiTiet in chiTietDonHangs)
                    {
                        chiTiet.MaDonHang = donHang.MaDonHang;

                        var sanPhams = await _context.SanPhams
                            .FirstOrDefaultAsync(sp => sp.MaSanPham == chiTiet.MaSanPham);

                        if (sanPhams == null)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = $"Sản phẩm với mã {chiTiet.MaSanPham} không tồn tại" };
                        }

                        if (sanPhams.SoLuong < chiTiet.SoLuong)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = $"Sản phẩm {sanPhams.TenSanPham} không đủ số lượng tồn kho" };
                        }

                        sanPhams.SoLuong -= chiTiet.SoLuong;
                    }
                    foreach (var support in donHangSupports)
                    {
                        support.ChiTietGioHang = chiTietDonHangs.FirstOrDefault()?.MaCtdh ?? 0;
                    }

                    _context.ChiTietDonHangs.AddRange(chiTietDonHangs);
                    _context.DonHangSupports.AddRange(donHangSupports);
                    await _context.SaveChangesAsync();

                    // Cập nhật coupon nếu có
                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .Include(c => c.MaVoucherNavigation)
                            .FirstOrDefaultAsync(c => c.MaNhap == request.CouponCode);
                        if (coupon != null)
                        {
                            coupon.TrangThai = 1;
                        }
                    }

                    // Cập nhật trạng thái đơn hàng thành "Đã thanh toán"
                    donHang.TrangThaiDonHang = TrangThaiDonHang.DaGiaoHang;
                    await _context.SaveChangesAsync();

                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        OrderId = donHang.MaDonHang,
                        Message = "Thanh toán tiền mặt thành công"
                    };
                }
                else if (request.PaymentMethod.ToLower() == "cod")
                {

                    var donHang = new DonHang
                    {
                        MaNguoiDung = orderDto.MaNguoiDung,
                        TenNguoiNhan = orderDto.TenNguoiNhan,
                        Sdt = orderDto.Sdt,
                        DiaChi = orderDto.DiaChi,
                        NgayDat = orderDto.NgayDat,
                        TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan,
                        TrangThaiHang = TrangThaiThanhToan.ThanhToanKhiNhanHang,
                        DiscountAmount = orderDto.DiscountAmount,
                        ShippingFee = orderDto.ShippingFee,
                        FinalAmount = orderDto.FinalAmount
                    };

                    _context.Add(donHang);
                    await _context.SaveChangesAsync();

                    foreach (var chiTiet in chiTietDonHangs)
                    {
                        chiTiet.MaDonHang = donHang.MaDonHang;

                        var sanPhams = await _context.SanPhams 
                            .FirstOrDefaultAsync(sp => sp.MaSanPham == chiTiet.MaSanPham);

                        if (sanPhams == null)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = $"Sản phẩm với mã {chiTiet.MaSanPham} không tồn tại" };
                        }

                        sanPhams.SoLuong -= chiTiet.SoLuong;
                    }
                    foreach (var support in donHangSupports)
                    {
                        support.ChiTietGioHang = chiTietDonHangs.FirstOrDefault()?.MaCtdh ?? 0;
                    }
                    var data = chiTietDonHangs;
                    data[0].SanPhamMaSanPham = data[0].MaSanPham;
                    _context.ChiTietDonHangs.Add(data[0]);
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .Include(c => c.MaVoucherNavigation)
                            .FirstOrDefaultAsync(c => c.MaNhap == request.CouponCode);
                        if (coupon == null)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = "Mã coupon không hợp lệ hoặc đã hết lượt sử dụng" };
                        }
                        coupon.TrangThai = 1;
                    }
                    await transaction.CommitAsync();
                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        OrderId = donHang.MaDonHang,
                        Message = "Thanh toán COD thành công"
                    };
                }
                else if (request.PaymentMethod.ToLower() == "vnpay")
                {
                    if (finalAmount <= 0)
                    {
                        await transaction.RollbackAsync();
                        return new PaymentResponse { Success = false, Message = "Số tiền thanh toán không hợp lệ cho VNPay" };
                    }
                    List<ChiTietGioHang> ChiTietGioHangs = new List<ChiTietGioHang>();
                    ChiTietGioHangs.Add(new ChiTietGioHang
                    {
                        MaSanPham = request.items[0].IdSanPham + "_" + request.items[0].MauSac + "_" + request.items[0].KickThuoc,
                        SoLuong = request.items[0].SoLuongMua,
                        Gia = request.items[0].TienSanPham,
                        ThanhTien = request.items[0].TienSanPham * request.items[0].SoLuongMua,
                        MaCombo = null,
                    });

                    foreach (var item in ChiTietGioHangs)
                    {
                        var sanPham = await _context.SanPhams
                            .FirstOrDefaultAsync(sp => sp.MaSanPham == item.MaSanPham);

                        if (sanPham == null)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = $"Sản phẩm với mã {item.MaSanPham} không tồn tại" };
                        }

                        if (sanPham.SoLuong < item.SoLuong)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = $"Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn kho" };
                        }
                    }

                    var tempOrderId = Guid.NewGuid().ToString();
                    var orderData = new
                    {
                        TempOrderId = tempOrderId,
                        Order = orderDto,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        CouponCode = request.CouponCode,
                        CartId = request.CartId,
                        ChiTietGioHangs,
                    };

                    var orderDataJson = System.Text.Json.JsonSerializer.Serialize(orderData, new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve,
                        WriteIndented = true
                    });

                    var pendingOrder = new PendingOder
                    {
                        TempOrderId = tempOrderId,
                        OrderData = orderDataJson,
                        CreatedAt = DateTime.Now,
                    };

                    _context.PendingOrders.Add(pendingOrder);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var vnPayRequest = new VnPaymentRequest
                    {
                        OrderId = tempOrderId,
                        FullName = orderDto.TenNguoiNhan,
                        Description = $"Thanh toán đơn hàng #{tempOrderId}",
                        Amount = Convert.ToDouble(Math.Ceiling(finalAmount)),
                        CreatedDate = DateTime.Now
                    };
                    _logger.LogInformation($"VNPay request: Amount={vnPayRequest.Amount}, OrderId={vnPayRequest.OrderId}");
                    var paymentUrl = _vnPayService.CreatePaymentUrl(httpContext, vnPayRequest);
                    _logger.LogInformation($"VNPay response: PaymentUrl={paymentUrl}");

                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        OrderId = 0,
                        Message = paymentUrl
                    };
                }
                await transaction.RollbackAsync();
                return new PaymentResponse { Success = false, Message = "Phương thức thanh toán không hợp lệ" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán: CartId={CartId}, PaymentMethod={PaymentMethod}", request.CartId, request.PaymentMethod);
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình thanh toán"
                };
            }
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequestDto request, HttpContext httpContext)
        {
            InstantBuy = false;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (request == null || request.CartId <= 0)
                {
                    return new PaymentResponse { Success = false, Message = "Yêu cầu thanh toán không hợp lệ" };
                }
                if (string.IsNullOrEmpty(request.PaymentMethod) || !new[] { "cash", "cod", "vnpay" }.Contains(request.PaymentMethod.ToLower()))
                {
                    return new PaymentResponse { Success = false, Message = "Phương thức thanh toán không hợp lệ" };
                }
                if (request.FinalAmount <= 0)
                {
                    return new PaymentResponse { Success = false, Message = "Số tiền cuối cùng không hợp lệ" };
                }

                _logger.LogInformation($"Processing payment: CartId={request.CartId}, PaymentMethod={request.PaymentMethod}, FinalAmount={request.FinalAmount}");

                var cart = await _context.GioHangs
                    .Include(c => c.ChiTietGioHangs)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                    .Include(c => c.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(c => c.MaGioHang == request.CartId);

                if (cart == null)
                {
                    return new PaymentResponse { Success = false, Message = "Giỏ hàng không tồn tại" };
                }
                if (!cart.ChiTietGioHangs.Any())
                {
                    return new PaymentResponse { Success = false, Message = "Giỏ hàng không chứa sản phẩm nào" };
                }

                decimal originalAmount = cart.ChiTietGioHangs.Sum(item => item.ThanhTien ?? 0);
                decimal discountAmount = request.DiscountAmount;
                decimal shippingFee = request.ShippingFee;
                decimal finalAmount = request.FinalAmount;

                var orderDto = new OrderDto
                {
                    MaNguoiDung = cart.MaNguoiDung,
                    TenNguoiNhan = request.TenNguoiNhan ?? cart.MaNguoiDungNavigation?.HoTen,
                    Sdt = request.Sdt ?? cart.MaNguoiDungNavigation?.Sdt,
                    DiaChi = request.DiaChi ?? cart.MaNguoiDungNavigation?.DiaChi,
                    NgayDat = DateTime.Now,
                    TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan,
                    TrangThaiHang = request.PaymentMethod.ToLower() switch
                    {
                        "cash" => TrangThaiThanhToan.ThanhToanTienMat,
                        "cod" => TrangThaiThanhToan.ThanhToanKhiNhanHang,
                        _ => TrangThaiThanhToan.ThanhToanVNPay
                    },
                    ChiTietDonHangs = new List<ChiTietDonHang>(),
                    DiscountAmount = discountAmount,
                    ShippingFee = shippingFee,
                    FinalAmount = finalAmount
                };

                if (string.IsNullOrEmpty(orderDto.TenNguoiNhan) || string.IsNullOrEmpty(orderDto.Sdt) || string.IsNullOrEmpty(orderDto.DiaChi))
                {
                    return new PaymentResponse { Success = false, Message = "Thông tin người nhận không hợp lệ" };
                }

                var chiTietDonHangs = new List<ChiTietDonHang>();
                var donHangSupports = new List<DonHangSupport>();
                var donHang = new DonHang
                {
                    MaNguoiDung = orderDto.MaNguoiDung,
                    TenNguoiNhan = orderDto.TenNguoiNhan,
                    Sdt = orderDto.Sdt,
                    DiaChi = orderDto.DiaChi,
                    NgayDat = orderDto.NgayDat,
                    TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan,
                    TrangThaiHang = orderDto.TrangThaiHang,
                    DiscountAmount = orderDto.DiscountAmount,
                    ShippingFee = orderDto.ShippingFee,
                    FinalAmount = orderDto.FinalAmount
                };
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();
                donHangTemp = donHang;

                var GioHang = _context.GioHangs.Where(g => g.MaNguoiDung == orderDto.MaNguoiDung).FirstOrDefault();
                var CTGH = _context.ChiTietGioHangs.Where(g => g.MaGioHang == GioHang.MaGioHang).ToList();

                // Validate stock before processing order details
                foreach (var item in CTGH)
                {
                    if (item.MaCombo != null)
                    {
                        var comboItems = await _context.ChiTietComBos
                            .Where(ct => ct.MaComBo == item.MaCombo)
                            .ToListAsync();
                        foreach (var comboItem in comboItems)
                        {
                            var sanPham = await _context.SanPhams
                                .FirstOrDefaultAsync(sp => sp.MaSanPham == comboItem.MaSanPham);
                            if (sanPham == null)
                            {
                                await transaction.RollbackAsync();
                                return new PaymentResponse { Success = false, Message = $"Sản phẩm với mã {comboItem.MaSanPham} không tồn tại" };
                            }
                            if (sanPham.SoLuong < comboItem.SoLuong * item.SoLuong)
                            {
                                await transaction.RollbackAsync();
                                return new PaymentResponse { Success = false, Message = $"Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn kho" };
                            }
                        }
                    }
                    else
                    {
                        var sanPham = await _context.SanPhams
                            .FirstOrDefaultAsync(sp => sp.MaSanPham == item.MaSanPham);
                        if (sanPham == null)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = $"Sản phẩm với mã {item.MaSanPham} không tồn tại" };
                        }
                        if (sanPham.SoLuong < item.SoLuong)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = $"Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn kho" };
                        }
                    }
                }

                foreach (var item in CTGH)
                {
                    ChiTietDonHang CTDH = new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        Gia = item.Gia,
                        SoLuong = item.SoLuong,
                        ThanhTien = item.ThanhTien
                    };

                    if (item.MaCombo != null)
                    {
                        CTDH.MaCombo = item.MaCombo;
                        _context.ChiTietDonHangs.Add(CTDH);
                        await _context.SaveChangesAsync();
                        chiTietDonHangs.Add(CTDH);

                        var CTCOMBO = await _context.ChiTietComBos
                            .Where(g => g.MaComBo == item.MaCombo)
                            .ToListAsync();
                        foreach (var item2 in CTCOMBO)
                        {
                            var GHSP = await _context.GioHangSupports
                                .Where(g => g.MaChiTietCombo == item2.MaChiTietComBo && g.ChiTietGioHang == item.MaCtgh)
                                .ToListAsync();
                            foreach (var item3 in GHSP)
                            {
                                DonHangSupport newDHSP = new DonHangSupport
                                {
                                    MaSanPham = item3.MaSanPham,
                                    MaChiTietCombo = item3.MaChiTietCombo,
                                    ChiTietGioHang = CTDH.MaCtdh,
                                    SoLuong = item3.SoLuong,
                                    Version = item3.Version
                                };
                                _context.DonHangSupports.Add(newDHSP);
                                await _context.SaveChangesAsync();
                                donHangSupports.Add(newDHSP);
                            }
                        }
                    }
                    else
                    {
                        CTDH.MaSanPham = item.MaSanPham;
                        _context.ChiTietDonHangs.Add(CTDH);
                        await _context.SaveChangesAsync();
                        chiTietDonHangs.Add(CTDH);
                    }
                }
                await _context.SaveChangesAsync();
                donHangSupportsTemp = donHangSupports;
                donHangTemp = donHang;
                chiTietDonHangTemp = chiTietDonHangs;

                if (request.PaymentMethod.ToLower() == "cash")
                {
                    // Reduce product quantities for cash payment
                    foreach (var item in CTGH)
                    {
                        if (item.MaCombo != null)
                        {
                            var comboItems = await _context.ChiTietComBos
                                .Where(ct => ct.MaComBo == item.MaCombo)
                                .ToListAsync();
                            foreach (var comboItem in comboItems)
                            {
                                var sanPham = await _context.SanPhams
                                    .FirstOrDefaultAsync(sp => sp.MaSanPham == comboItem.MaSanPham);
                                sanPham.SoLuong -= comboItem.SoLuong * item.SoLuong;
                            }
                        }
                        else
                        {
                            var sanPham = await _context.SanPhams
                                .FirstOrDefaultAsync(sp => sp.MaSanPham == item.MaSanPham);
                            sanPham.SoLuong -= item.SoLuong;
                        }
                    }

                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .Include(c => c.MaVoucherNavigation)
                            .FirstOrDefaultAsync(c => c.MaNhap == request.CouponCode);
                        if (coupon != null)
                        {
                            coupon.TrangThai = 1;
                        }
                    }

                    donHang.TrangThaiDonHang = TrangThaiDonHang.DaGiaoHang;
                    await _context.SaveChangesAsync();

                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        OrderId = donHang.MaDonHang,
                        Message = "Thanh toán tiền mặt thành công"
                    };
                }
                else if (request.PaymentMethod.ToLower() == "cod")
                {
                    // Reduce product quantities for COD payment
                    foreach (var item in CTGH)
                    {
                        if (item.MaCombo != null)
                        {
                            var comboItems = await _context.ChiTietComBos
                                .Where(ct => ct.MaComBo == item.MaCombo)
                                .ToListAsync();
                            foreach (var comboItem in comboItems)
                            {
                                var sanPham = await _context.SanPhams
                                    .FirstOrDefaultAsync(sp => sp.MaSanPham == comboItem.MaSanPham);
                                sanPham.SoLuong -= comboItem.SoLuong * item.SoLuong;
                            }
                        }
                        else
                        {
                            var sanPham = await _context.SanPhams
                                .FirstOrDefaultAsync(sp => sp.MaSanPham == item.MaSanPham);
                            sanPham.SoLuong -= item.SoLuong;
                        }
                    }

                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .Include(c => c.MaVoucherNavigation)
                            .FirstOrDefaultAsync(c => c.MaNhap == request.CouponCode);
                        if (coupon == null)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = "Mã coupon không hợp lệ hoặc đã hết lượt sử dụng" };
                        }
                        coupon.TrangThai = 1;
                    }

                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        OrderId = donHang.MaDonHang,
                        Message = "Thanh toán COD thành công"
                    };
                }
                else if (request.PaymentMethod.ToLower() == "vnpay")
                {
                    if (finalAmount <= 0)
                    {
                        await transaction.RollbackAsync();
                        return new PaymentResponse { Success = false, Message = "Số tiền thanh toán không hợp lệ cho VNPay" };
                    }

                    var tempOrderId = Guid.NewGuid().ToString();
                    var orderData = new
                    {
                        TempOrderId = tempOrderId,
                        Order = orderDto,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        CouponCode = request.CouponCode,
                        CartId = request.CartId,
                        ChiTietGioHangs = cart.ChiTietGioHangs.Select(item => new
                        {
                            item.MaSanPham,
                            item.SoLuong,
                            item.Gia,
                            item.ThanhTien,
                            item.MaCombo,
                            item.MaCtgh
                        }).ToList()
                    };

                    var orderDataJson = System.Text.Json.JsonSerializer.Serialize(orderData, new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve,
                        WriteIndented = true
                    });

                    var pendingOrder = new PendingOder
                    {
                        TempOrderId = tempOrderId,
                        OrderData = orderDataJson,
                        CreatedAt = DateTime.Now,
                    };
                    donHangTemp = donHang;
                    _context.PendingOrders.Add(pendingOrder);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var vnPayRequest = new VnPaymentRequest
                    {
                        OrderId = tempOrderId,
                        FullName = orderDto.TenNguoiNhan,
                        Description = $"Thanh toán đơn hàng #{tempOrderId}",
                        Amount = Convert.ToDouble(Math.Ceiling(finalAmount)),
                        CreatedDate = DateTime.Now
                    };

                    _logger.LogInformation($"VNPay request: Amount={vnPayRequest.Amount}, OrderId={vnPayRequest.OrderId}");
                    var paymentUrl = _vnPayService.CreatePaymentUrl(httpContext, vnPayRequest);
                    _logger.LogInformation($"VNPay response: PaymentUrl={paymentUrl}");

                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        OrderId = 0,
                        Message = paymentUrl
                    };
                }

                await transaction.RollbackAsync();
                return new PaymentResponse { Success = false, Message = "Phương thức thanh toán không hợp lệ" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán: CartId={CartId}, PaymentMethod={PaymentMethod}", request.CartId, request.PaymentMethod);
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình thanh toán"
                };
            }
        }


        public async Task ProcessVnPayCallbackAsync(IQueryCollection query, HttpContext httpContext)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var vnPayResponse = _vnPayService.PaymentExecute(query);

                if (!vnPayResponse.Success)
                {
                    _context.DonHangSupports.RemoveRange(donHangSupportsTemp);
                    _context.ChiTietDonHangs.RemoveRange(chiTietDonHangTemp);
                    _context.DonHangs.RemoveRange(donHangTemp);
                    await _context.SaveChangesAsync();
                    string message = vnPayResponse.VnPayResponseCode switch
                    {
                        "01" => "Giao dich chua hoan tat (nguoi dung huy)",
                        "02" => "Giao dich bi loi",
                        "24" => "Giao dich bi huy boi nguoi dung",
                        _ => "Thanh toan VNPay khong thanh cong"
                    };

                    message = System.Text.RegularExpressions.Regex.Replace(message, "[^\\x00-\\x7F]", "");
                    httpContext.Response.Redirect(
                        $"http://localhost:8080/PaymentFail?status=failed&message={Uri.EscapeDataString(message)}"
                    );
                    return;
                }

                var tempOrderId = vnPayResponse.OrderId;
                var pendingOrder = await _context.PendingOrders.FirstOrDefaultAsync(c => c.TempOrderId == tempOrderId);
                if (pendingOrder == null)
                {
                    httpContext.Response.Redirect("http://localhost:8080/PaymentFail?status=failed&message=Khong tim thay ma don hang tam thoi");
                    return;
                }

                var orderData = System.Text.Json.JsonSerializer.Deserialize<PendingVnPayOrder>(pendingOrder.OrderData,
                    new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve,
                        PropertyNameCaseInsensitive = true
                    });

                if (orderData.TempOrderId != tempOrderId)
                {
                    httpContext.Response.Redirect("http://localhost:8080/PaymentFail?status=failed&message=Ma don hang tam thoi khong khop");
                    return;
                }

                // Validate stock before processing
                foreach (var item in orderData.ChiTietGioHangs)
                {
                    if (item.MaCombo != null)
                    {
                        var comboItems = await _context.ChiTietComBos
                            .Where(ct => ct.MaComBo == item.MaCombo)
                            .ToListAsync();
                        foreach (var comboItem in comboItems)
                        {
                            var sanPham = await _context.SanPhams
                                .FirstOrDefaultAsync(sp => sp.MaSanPham == comboItem.MaSanPham);
                            if (sanPham == null)
                            {
                                await transaction.RollbackAsync();
                                httpContext.Response.Redirect(
                                    $"http://localhost:8080/PaymentFail?status=failed&message=Sản phẩm với mã {comboItem.MaSanPham} không tồn tại"
                                );
                                return;
                            }
                            if (sanPham.SoLuong < comboItem.SoLuong * item.SoLuong)
                            {
                                await transaction.RollbackAsync();
                                httpContext.Response.Redirect(
                                    $"http://localhost:8080/PaymentFail?status=failed&message=Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn kho"
                                );
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.MaSanPham))
                        {
                            _logger.LogWarning($"ChiTietGioHangDto có MaSanPham null: {JsonSerializer.Serialize(item)}");
                            continue;
                        }
                        var sanPham = await _context.SanPhams
                            .FirstOrDefaultAsync(sp => sp.MaSanPham == item.MaSanPham);
                        if (sanPham == null)
                        {
                            await transaction.RollbackAsync();
                            httpContext.Response.Redirect(
                                $"http://localhost:8080/PaymentFail?status=failed&message=Sản phẩm với mã {item.MaSanPham} không tồn tại"
                            );
                            return;
                        }
                        if (sanPham.SoLuong < item.SoLuong)
                        {
                            await transaction.RollbackAsync();
                            httpContext.Response.Redirect(
                                $"http://localhost:8080/PaymentFail?status=failed&message=Sản phẩm {sanPham.TenSanPham} không đủ số lượng tồn kho"
                            );
                            return;
                        }
                    }
                }

                // Reduce product quantities
                foreach (var item in orderData.ChiTietGioHangs)
                {
                    if (item.MaCombo != null)
                    {
                        var comboItems = await _context.ChiTietComBos
                            .Where(ct => ct.MaComBo == item.MaCombo)
                            .ToListAsync();
                        foreach (var comboItem in comboItems)
                        {
                            var sanPham = await _context.SanPhams
                                .FirstOrDefaultAsync(sp => sp.MaSanPham == comboItem.MaSanPham);
                            if (sanPham != null)
                            {
                                sanPham.SoLuong -= comboItem.SoLuong * item.SoLuong;
                                _logger.LogInformation($"Reduced quantity for product {sanPham.MaSanPham} by {comboItem.SoLuong * item.SoLuong} for order {donHangTemp.MaDonHang}");
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.MaSanPham))
                        {
                            _logger.LogWarning($"ChiTietGioHangDto có MaSanPham null: {JsonSerializer.Serialize(item)}");
                            continue;
                        }
                        var sanPham = await _context.SanPhams
                            .FirstOrDefaultAsync(sp => sp.MaSanPham == item.MaSanPham);
                        if (sanPham != null)
                        {
                            sanPham.SoLuong -= item.SoLuong;
                            _logger.LogInformation($"Reduced quantity for product {sanPham.MaSanPham} by {item.SoLuong} for order {donHangTemp.MaDonHang}");
                        }
                    }
                }

                if (InstantBuy)
                {
                    donHangTemp.ChiTietDonHangs[0].SanPhamMaSanPham = donHangTemp.ChiTietDonHangs[0].MaSanPham;
                    _context.DonHangs.Add(donHangTemp);
                    await _context.SaveChangesAsync();
                    InstantBuy = true;
                    var chiTietDonHangs = orderData.ChiTietGioHangs
                        .Select(item =>
                        {
                            if (item.MaSanPham == null)
                            {
                                _logger.LogWarning($"ChiTietGioHangDto có MaSanPham null: {JsonSerializer.Serialize(item)}");
                            }
                            return item;
                        })
                        .Where(item => item.MaSanPham != null)
                        .Select(item => new ChiTietDonHang
                        {
                            MaSanPham = item.MaSanPham.ToString(),
                            SoLuong = item.SoLuong,
                            Gia = (int?)item.Gia,
                            ThanhTien = (int?)item.ThanhTien,
                            MaCombo = item.MaCombo,
                            SanPhamMaSanPham = item.MaSanPham.ToString(),
                            MaDonHang = donHangTemp.MaDonHang
                        }).ToList();
                    _context.ChiTietDonHangs.AddRange(chiTietDonHangs);
                }

                if (!string.IsNullOrEmpty(orderData.CouponCode))
                {
                    var coupon = await _context.Coupons
                        .Include(c => c.MaVoucherNavigation)
                        .FirstOrDefaultAsync(c => c.MaNhap == orderData.CouponCode);
                    if (coupon != null)
                    {
                        coupon.TrangThai = 1;
                    }
                }

                var cartId = orderData.CartId;
                var cart = await _context.GioHangs
                    .Include(c => c.ChiTietGioHangs)
                    .FirstOrDefaultAsync(c => c.MaGioHang == cartId);

                if (cart != null && !InstantBuy)
                {
                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
                }
                _context.PendingOrders.Remove(pendingOrder);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var redirectUrl = $"http://localhost:8080/PaymentSuccess?status=success&orderId={donHangTemp.MaDonHang}&transactionId={vnPayResponse.TransactionId}";
                Console.WriteLine($"Redirecting to: {redirectUrl}");
                httpContext.Response.Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xử lý callback VNPay");
                string errorMessage = "Loi khi xu ly callback VNPay";
                httpContext.Response.Redirect(
                    $"http://localhost:8080/PaymentFail?status=failed&message={Uri.EscapeDataString(errorMessage)}"
                );
            }
        }
    }
}
