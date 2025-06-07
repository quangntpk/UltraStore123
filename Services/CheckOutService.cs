using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
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

                decimal discountAmount = request.DiscountAmount;
                decimal finalAmount = request.FinalAmount;
                decimal shippingFee = request.ShippingFee;

                if (!string.IsNullOrEmpty(request.CouponCode))
                {
                    var coupon = await _context.Coupons
                        .Include(c => c.MaVoucherNavigation)
                        .FirstOrDefaultAsync(d => d.MaNhap == request.CouponCode && d.TrangThai == 0);
                    if (coupon == null)
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Coupon không hợp lệ hoặc đã được sử dụng"
                        };
                    }

                    var voucher = coupon.MaVoucherNavigation;
                    var now = DateTime.Now;

                    if (voucher.TrangThai != 0
                        || voucher.NgayBatDau > now
                        || voucher.NgayKetThuc < now
                        || voucher.SoLuong <= 0
                        || originalAmount < (voucher.DieuKien ?? 0))
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Coupon không hợp lệ hoặc đã hết hạn"
                        };
                    }

                    decimal calculatedFinal = originalAmount - discountAmount + shippingFee;

                    if (calculatedFinal != finalAmount)
                    {
                        _logger.LogWarning(
                            $"Tổng tiền không khớp. CalculatedFinal: {calculatedFinal}, FinalAmount: {finalAmount}, ShippingFee: {shippingFee}, DiscountAmount: {discountAmount}"
                        );
                    }
                }
                else
                {
                    if (finalAmount != originalAmount + shippingFee)
                    {
                        _logger.LogWarning(
                            $"Tổng tiền không khớp khi không có mã giảm giá. Expected: {originalAmount + shippingFee}, FinalAmount: {finalAmount}"
                        );
                    }
                }

                var donHang = new DonHang
                {
                    MaNguoiDung = cart.MaNguoiDung,
                    TenNguoiNhan = request.TenNguoiNhan ?? cart.MaNguoiDungNavigation?.HoTen,
                    Sdt = request.Sdt ?? cart.MaNguoiDungNavigation?.Sdt,
                    DiaChi = request.DiaChi ?? cart.MaNguoiDungNavigation?.DiaChi,
                    NgayDat = DateTime.Now,
                    TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan, // Sẽ cập nhật sau
                    TrangThaiHang = request.PaymentMethod.ToLower() == "cash"
              ? TrangThaiThanhToan.ThanhToanTienMat
              : request.PaymentMethod.ToLower() == "cod"
                  ? TrangThaiThanhToan.ThanhToanKhiNhanHang
                  : TrangThaiThanhToan.ThanhToanVNPay,
                    ChiTietDonHangs = new List<ChiTietDonHang>(),
                    DiscountAmount = discountAmount,
                    ShippingFee = shippingFee,
                    FinalAmount = finalAmount
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

                if (request.PaymentMethod.ToLower() == "cash")
                {
                    // Thanh toán offline bằng tiền mặt
                    _context.DonHangs.Add(donHang);

                    // Cập nhật coupon nếu có
                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .Include(c => c.MaVoucherNavigation)
                            .FirstOrDefaultAsync(c => c.MaNhap == request.CouponCode);
                        if (coupon != null)
                        {
                            coupon.TrangThai = 1;
                            coupon.MaVoucherNavigation.SoLuong -= 1;
                        }
                    }

                    // Cập nhật trạng thái đơn hàng thành "Đã thanh toán"
                    donHang.TrangThaiDonHang = TrangThaiDonHang.DaGiaoHang;
                    await _context.SaveChangesAsync();

                    // Xóa giỏ hàng
                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
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
                    _context.DonHangs.Add(donHang);

                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .Include(c => c.MaVoucherNavigation)
                            .FirstOrDefaultAsync(c => c.MaNhap == request.CouponCode);
                        if (coupon != null)
                        {
                            coupon.TrangThai = 1;
                            coupon.MaVoucherNavigation.SoLuong -= 1;
                        }
                    }

                    await _context.SaveChangesAsync();

                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
                    await _context.SaveChangesAsync();

                    return new PaymentResponse
                    {
                        Success = true,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        OrderId = donHang.MaDonHang,
                        Message = "Đặt hàng COD thành công"
                    };
                }
                else if (request.PaymentMethod.ToLower() == "vnpay")
                {
                    var tempOrderId = Guid.NewGuid().ToString();

                    var orderData = new
                    {
                        TempOrderId = tempOrderId,
                        Order = donHang,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = request.ShippingFee,
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
                        ShippingFee = request.ShippingFee,
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
                    string message = vnPayResponse.VnPayResponseCode switch
                    {
                        "01" => "Giao dich chua hoan tat (nguoi dung huy)",
                        "02" => "Giao dich bi loi",
                        "24" => "Giao dịch bị hủy bởi người dùng",
                        _ => "Thanh toan VNPay khong thanh cong"
                    };
                    httpContext.Response.Redirect(
                        $"http://localhost:8080/PaymentFail?status=failed&message={Uri.EscapeDataString(message)}"
                    );
                    return;
                }

                var tempOrderId = vnPayResponse.OrderId;

                var pendingOrder = await _context.PendingOrders.FirstOrDefaultAsync(c => c.TempOrderId == tempOrderId);
                if (pendingOrder == null)
                {
                    httpContext.Response.Redirect("http://localhost:8080/PaymentFail?status=failed&message=Không tìm thấy mã đơn hàng tạm thời");
                    return;
                }

                var orderData = System.Text.Json.JsonSerializer.Deserialize<PendingVnPayOrder>(pendingOrder.OrderData);

                if (orderData.TempOrderId != tempOrderId)
                {
                    httpContext.Response.Redirect("http://localhost:8080/PaymentFail?status=failed&message=Mã đơn hàng tạm thời không khớp");
                    return;
                }

                var donHang = orderData.Order;
                donHang.TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan;
                donHang.TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay;
                donHang.DiscountAmount = orderData.DiscountAmount;
                donHang.ShippingFee = orderData.ShippingFee;
                donHang.FinalAmount = orderData.FinalAmount;

                _context.DonHangs.Add(donHang);

                if (!string.IsNullOrEmpty(orderData.CouponCode))
                {
                    var coupon = await _context.Coupons
                        .Include(c => c.MaVoucherNavigation)
                        .FirstOrDefaultAsync(c => c.MaNhap == orderData.CouponCode);
                    if (coupon != null)
                    {
                        coupon.TrangThai = 1;
                        coupon.MaVoucherNavigation.SoLuong -= 1;
                    }
                }

                await _context.SaveChangesAsync();

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
                httpContext.Response.Redirect("http://localhost:8080/PaymentFail?status=failed&message=Lỗi khi xử lý callback VnPay");
            }
        }
    }
}