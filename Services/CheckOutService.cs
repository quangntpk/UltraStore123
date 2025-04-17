using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.DTO;
using Microsoft.AspNetCore.Http;
using UltraStrore.Repository;
using System.Net.Http;
using UltraStrore.Data.Temp;

namespace UltraStrore.Services
{
    public class CheckOutService : ICheckOutServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckOutService> _logger;
        private readonly VnPayConfig _vnpayConfig;
        private readonly IVnPayServies _vnPayService;

        public CheckOutService(ApplicationDbContext context, ILogger<CheckOutService> logger, VnPayConfig vnpayConfig, IVnPayServies vnPayService)
        {
            _context = context;
            _logger = logger;
            _vnpayConfig = vnpayConfig;
            _vnPayService = vnPayService;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequestDto request, HttpContext httpContext)
        {
            try
            {
                var cart = await _context.GioHangs
                    .Include(c => c.ChiTietGioHangs)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                    .Include(c => c.MaNguoiDungNavigation)
                    .FirstOrDefaultAsync(c => c.MaGioHang == request.CartId);

                if (cart == null)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Giỏ hàng không tồn tại"
                    };
                }

                decimal originalAmount = cart.ChiTietGioHangs
                    .Sum(item => item.ThanhTien ?? 0);

                decimal discountAmount = 0;
                decimal finalAmount = originalAmount;

                if (!string.IsNullOrEmpty(request.CouponCode))
                {
                    var coupon = await _context.Coupons
                        .Include(c => c.MaVoucherNavigation)
                        .FirstOrDefaultAsync(d => d.MaNhap == request.CouponCode && d.TrangThai == 0);
                    var voucher = coupon.MaVoucherNavigation;
                    var now = DateTime.Now;

                    if (voucher.TrangThai == 0
                        && voucher.NgayBatDau <= now
                        && voucher.NgayKetThuc >= now
                        && voucher.SoLuong > 0
                        && originalAmount >= (voucher.DieuKien ?? 0))
                    {
                        decimal discountPercentage = (decimal)(voucher.GiaTri ?? 0);
                        discountAmount = originalAmount * (discountPercentage / 100);
                        finalAmount = originalAmount - discountAmount;

                        if (finalAmount < 0)
                        {
                            finalAmount = 0;
                            discountAmount = originalAmount;
                        }

                        coupon.TrangThai = 1;
                        voucher.SoLuong -= 1;
                    }
                    else
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Coupon không hợp lệ hoặc đã hết hạn"
                        };
                    }
                }

                var donHang = new DonHang
                {
                    MaNguoiDung = cart.MaNguoiDung,
                    TenNguoiNhan = request.TenNguoiNhan,
                    Sdt = request.Sdt,
                    DiaChi = request.DiaChi,
                    NgayDat = DateTime.Now,
                    TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan,
                    TrangThaiHang = request.PaymentMethod.ToLower() == "cod"
                        ? TrangThaiThanhToan.ThanhToanKhiNhanHang
                        : TrangThaiThanhToan.ThanhToanVNPay,
                    ChiTietDonHangs = new List<ChiTietDonHang>()
                };

                foreach (var item in cart.ChiTietGioHangs)
                {
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        MaSanPham = item.MaSanPham,
                        SoLuong = item.SoLuong,
                        Gia = item.Gia,
                        ThanhTien = item.ThanhTien,
                        MaCombo = item.MaCombo,
                        SanPhamMaSanPham = item.MaSanPham
                    };
                    donHang.ChiTietDonHangs.Add(chiTietDonHang);
                }


                if (request.PaymentMethod.ToLower() == "cod")
                {
                    donHang.TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan;
                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync();

                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .FirstOrDefaultAsync(d => d.MaNhap == request.CouponCode && d.TrangThai == 0);

                        if (coupon != null)
                        {
                            coupon.TrangThai = 1;
                            var voucher = coupon.MaVoucherNavigation;
                            voucher.SoLuong -= 1;
                            await _context.SaveChangesAsync();
                        }
                    }

                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        FinalAmount = finalAmount,
                        OrderId = donHang.MaDonHang,
                        Message = "Đặt hàng COD thành công"
                    };
                }
                else if (request.PaymentMethod.ToLower() == "vnpay")
                {
                    var donhang = new DonHang
                    {
                        MaNguoiDung = cart.MaNguoiDung,
                        TenNguoiNhan = request.TenNguoiNhan,
                        Sdt = request.Sdt,
                        DiaChi = request.DiaChi,
                        NgayDat = DateTime.Now,
                        TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan,
                        TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay,
                        ChiTietDonHangs = cart.ChiTietGioHangs.Select(item => new ChiTietDonHang
                        {
                            MaSanPham = item.MaSanPham,
                            SoLuong = item.SoLuong,
                            Gia = item.Gia,
                            ThanhTien = item.ThanhTien,
                            MaCombo = item.MaCombo,
                            SanPhamMaSanPham = item.MaSanPham
                        }).ToList()
                    };

                    var tempOrderId = Guid.NewGuid().ToString();

                    var orderData = new
                    {
                        TempOrderId = tempOrderId,
                        Order = donHang,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        FinalAmount = finalAmount,
                        CouponCode = request.CouponCode,
                        CartId = request.CartId
                    };

                    var orderDataJson = System.Text.Json.JsonSerializer.Serialize(orderData);             

                    var pendingOrder = new PendingOder
                    {
                        TempOrderId = tempOrderId,
                        OrderData = orderDataJson,
                        CreatedAt = DateTime.Now,
                    };

                    _context.PendingOrders.Add(pendingOrder);   
                    await _context.SaveChangesAsync();

                    var saveOrder = await _context.PendingOrders.FirstOrDefaultAsync( c => c.TempOrderId == tempOrderId);
                    if (saveOrder == null)
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Loi khi luu don hang tam thoi"
                        };
                    }

                    var vnPayRequest = new VnPaymentRequest
                    {
                        OrderId = tempOrderId,
                        FullName = donHang.TenNguoiNhan,
                        Description = $"Thanh toán đơn hàng #{donHang.MaDonHang}",
                        Amount = (double)finalAmount,
                        CreatedDate = DateTime.Now
                    };

                    var paymentUrl = _vnPayService.CreatePaymentUrl(httpContext, vnPayRequest);

                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        FinalAmount = finalAmount,
                        OrderId = donHang.MaDonHang,
                        Message = paymentUrl
                    };
                }
                else
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Phương thức thanh toán không được hỗ trợ"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán");
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình thanh toán"
                };
            }
        }

        public async Task ProcessVnPayCallbackAsync(IQueryCollection query, HttpContext httpContext)
        {
            try
            {
                var vnPayResponse = _vnPayService.PaymentExecute(query);           
                if (!vnPayResponse.Success)
                {

                    httpContext.Response.Redirect("http://localhost:8080/PaymentSuccess?status=failed&message=Thanh toán VnPay thất bại");
                    return;
                }

                var tempOrderId = vnPayResponse.OrderId;

               var pendingOrder = await _context.PendingOrders.FirstOrDefaultAsync(c => c.TempOrderId == tempOrderId);
                if (pendingOrder == null)
                {
                    httpContext.Response.Redirect("http://localhost:8080/PaymentSuccess?status=failed&message=Không tìm thấy mã đơn hàng tạm thời");
                    return;
                }

                var orderData = System.Text.Json.JsonSerializer.Deserialize<PendingVnPayOrder>(pendingOrder.OrderData);

                if (orderData.TempOrderId != tempOrderId ) 
                {
                    httpContext.Response.Redirect("http://localhost:8080/PaymentSuccess?status=failed&message=Mã đơn hàng tạm thời không khớp");
                    return;
                }


                var donHang = orderData.Order;
                donHang.TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan;
                donHang.TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay;


                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(orderData.CouponCode))
                {
                    var coupon = await _context.Coupons
                        .Include(c => c.MaVoucherNavigation)
                        .FirstOrDefaultAsync(d => d.MaNhap == orderData.CouponCode && d.TrangThai == 0);

                    if (coupon != null)
                    {
                        coupon.TrangThai = 1;
                        var voucher = coupon.MaVoucherNavigation;
                        voucher.SoLuong -= 1;
                        await _context.SaveChangesAsync();
                    }
                }

                var cartId = (int)orderData.CartId;
                var cart = await _context.GioHangs
                    .Include(c => c.ChiTietGioHangs)
                    .FirstOrDefaultAsync(c => c.MaGioHang == cartId);
                if (cart != null)
                {
                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
                    await _context.SaveChangesAsync();
                }

                _context.PendingOrders.Remove(pendingOrder);
                await _context.SaveChangesAsync();

                var redirectUrl = $"http://localhost:8080/PaymentSuccess?status=success&orderId={donHang.MaDonHang}&transactionId={vnPayResponse.TransactionId}";
                httpContext.Response.Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý callback VnPay");
                httpContext.Response.Redirect("http://localhost:8080/PaymentSuccess?status=failed&message=Lỗi khi xử lý callback VnPay");
            }
        }
    }
}