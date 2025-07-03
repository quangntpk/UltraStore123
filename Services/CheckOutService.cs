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
            using var transaction = await _context.Database.BeginTransactionAsync();
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

                if (!cart.ChiTietGioHangs.Any())
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Giỏ hàng không chứa sản phẩm nào"
                    };
                }

                decimal originalAmount = cart.ChiTietGioHangs
                    .Sum(item => item.ThanhTien ?? 0);

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
                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();
                foreach (var item in cart.ChiTietGioHangs)
                {
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        MaSanPham = item.MaSanPham,
                        SoLuong = item.SoLuong,
                        Gia = item.Gia,
                        ThanhTien = item.ThanhTien,
                        MaCombo = item.MaCombo,
                        SanPhamMaSanPham = item.MaSanPham,
                        MaDonHang = donHang.MaDonHang 
                    };
                    _context.ChiTietDonHangs.Add(chiTietDonHang);
                    await _context.SaveChangesAsync();
                    var Test = chiTietDonHang;
                    var ChiTietGioHangsp = _context.GioHangSupports.Where(g => g.ChiTietGioHang == item.MaCtgh && item.MaCombo!=null).ToList();
                    foreach (var k in ChiTietGioHangsp)
                    {
                        var GHsupport = new DonHangSupport
                        {
                            MaSanPham = k.MaSanPham,
                            ChiTietGioHang = chiTietDonHang.MaCtdh,
                            MaChiTietCombo = k.MaChiTietCombo,
                            SoLuong = k.SoLuong,
                        };
                         _context.DonHangSupports.Add(GHsupport);
                    }
                    await _context.SaveChangesAsync();
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
                    await transaction.CommitAsync();
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
                        Order = orderDto,
                        OriginalAmount = originalAmount,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount,
                        CouponCode = request.CouponCode,
                        CartId = request.CartId,

                          ChiTietGioHangs = cart.ChiTietGioHangs.Select(item => new {
                              item.MaSanPham,
                              item.SoLuong,
                              item.Gia,
                              item.ThanhTien,
                              item.MaCombo,
                              item.MaCtgh
                          }).ToList()
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
                    await transaction.CommitAsync();

                    var vnPayRequest = new VnPaymentRequest
                    {
                        OrderId = tempOrderId,
                        FullName = orderDto.TenNguoiNhan,
                        Description = $"Thanh toán đơn hàng #{tempOrderId}",
                        Amount = Convert.ToDouble(Math.Ceiling(finalAmount)),
                        CreatedDate = DateTime.Now
                    };

                    var paymentUrl = _vnPayService.CreatePaymentUrl(httpContext, vnPayRequest);

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
                else
                {
                    var donHang = new DonHang
                    {
                        MaNguoiDung = orderDto.MaNguoiDung,
                        TenNguoiNhan = orderDto.TenNguoiNhan,
                        Sdt = orderDto.Sdt,
                        DiaChi = orderDto.DiaChi,
                        NgayDat = DateTime.Now,
                        TrangThaiDonHang = request.PaymentMethod.ToLower() == "cod"
                            ? TrangThaiDonHang.DaGiaoHang
                            : TrangThaiDonHang.ChuaXacNhan,
                        TrangThaiHang = request.PaymentMethod.ToLower() == "cod"
                            ? TrangThaiThanhToan.ThanhToanTienMat
                            : TrangThaiThanhToan.ThanhToanKhiNhanHang,
                        DiscountAmount = discountAmount,
                        ShippingFee = shippingFee,
                        FinalAmount = finalAmount
                    };

                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync();

                    var chiTietDonHangs = cart.ChiTietGioHangs.Select(item => new ChiTietDonHang
                    {
                        MaSanPham = item.MaSanPham,
                        SoLuong = item.SoLuong,
                        Gia = item.Gia,
                        ThanhTien = item.ThanhTien,
                        MaCombo = item.MaCombo,
                        SanPhamMaSanPham = item.MaSanPham,
                        MaDonHang = donHang.MaDonHang
                    }).ToList();

                    _context.ChiTietDonHangs.AddRange(chiTietDonHangs);
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
                        Message = request.PaymentMethod.ToLower() == "cash"
                            ? "Thanh toán tiền mặt thành công"
                            : "Đặt hàng COD thành công"
                    };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var vnPayResponse = _vnPayService.PaymentExecute(query);

                if (!vnPayResponse.Success)
                {
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

                var orderData = System.Text.Json.JsonSerializer.Deserialize<PendingVnPayOrder>(pendingOrder.OrderData);

                if (orderData.TempOrderId != tempOrderId)
                {
                    httpContext.Response.Redirect("http://localhost:8080/PaymentFail?status=failed&message=Ma don hang tam thoi khong khop");
                    return;
                }

                var donHang = orderData.Order;
                donHang.TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan;
                donHang.TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay;
                donHang.DiscountAmount = orderData.DiscountAmount;
                donHang.ShippingFee = orderData.ShippingFee;
                donHang.FinalAmount = orderData.FinalAmount;

                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();


                var chiTietDonHangs = orderData.ChiTietGioHangs.Select(item => new ChiTietDonHang
                {
                    MaSanPham = item.MaSanPham.ToString(),
                    SoLuong = item.SoLuong,
                    Gia = (int?)item.Gia,                
                    ThanhTien = (int?)item.ThanhTien,     
                    MaCombo = item.MaCombo,               
                    SanPhamMaSanPham = item.MaSanPham.ToString(),
                    MaDonHang = donHang.MaDonHang
                }).ToList();

                _context.ChiTietDonHangs.AddRange(chiTietDonHangs);

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

                var cartId = orderData.CartId;
                var cart = await _context.GioHangs
                    .Include(c => c.ChiTietGioHangs)
                    .FirstOrDefaultAsync(c => c.MaGioHang == cartId);

                if (cart != null)
                {
                    _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);
                    _context.GioHangs.Remove(cart);
                }
                _context.PendingOrders.Remove(pendingOrder);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var redirectUrl = $"http://localhost:8080/PaymentSuccess?status=success&orderId={donHang.MaDonHang}&transactionId={vnPayResponse.TransactionId}";
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