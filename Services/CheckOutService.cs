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
                    };
                    chiTietDonHangs.Add(chiTietDonHang);
                    orderDto.ChiTietDonHangs.Add(chiTietDonHang);

                    var chiTietGioHangSupports = _context.GioHangSupports
                        .Where(g => g.ChiTietGioHang == item.MaCtgh && item.MaCombo != null)
                        .ToList();

                    foreach (var k in chiTietGioHangSupports)
                    {
                        var donHangSupport = new DonHangSupport
                        {
                            MaSanPham = k.MaSanPham,
                            ChiTietGioHang = chiTietDonHang.MaCtdh,
                            MaChiTietCombo = k.MaChiTietCombo,
                            SoLuong = k.SoLuong,
                        };
                        donHangSupports.Add(donHangSupport);
                    }
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
                    }
                    foreach (var support in donHangSupports)
                    {
                        support.ChiTietGioHang = chiTietDonHangs.FirstOrDefault()?.MaCtdh ?? 0;
                    }

                    _context.ChiTietDonHangs.AddRange(chiTietDonHangs);
                    _context.DonHangSupports.AddRange(donHangSupports);

                    if (!string.IsNullOrEmpty(request.CouponCode))
                    {
                        var coupon = await _context.Coupons
                            .Include(c => c.MaVoucherNavigation)
                            .FirstOrDefaultAsync(c => c.MaNhap == request.CouponCode);
                        if (coupon == null || coupon.MaVoucherNavigation.SoLuong <= 0)
                        {
                            await transaction.RollbackAsync();
                            return new PaymentResponse { Success = false, Message = "Mã coupon không hợp lệ hoặc đã hết lượt sử dụng" };
                        }
                        coupon.TrangThai = 1;
                        coupon.MaVoucherNavigation.SoLuong -= 1;
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

                var donHang = orderData.Order;
                donHang.TrangThaiDonHang = TrangThaiDonHang.ChuaXacNhan;
                donHang.TrangThaiHang = TrangThaiThanhToan.ThanhToanVNPay;
                donHang.DiscountAmount = orderData.DiscountAmount;
                donHang.ShippingFee = orderData.ShippingFee;
                donHang.FinalAmount = orderData.FinalAmount;

                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync();


                var chiTietDonHangs = orderData.ChiTietGioHangs.Select(item =>
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