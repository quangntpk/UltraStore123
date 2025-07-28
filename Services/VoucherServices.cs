using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using UltraStrore.Helper;

public class VoucherServices : IVoucherServices
{
    private readonly ApplicationDbContext _context;

    public VoucherServices(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<VoucherView> GetAllVouchers()
    {
        return _context.Vouchers
            .Select(v => new VoucherView
            {
                MaVoucher = v.MaVoucher,
                TenVoucher = v.TenVoucher,
                GiaTri = v.GiaTri,
                MoTa = v.MoTa,
                NgayBatDau = v.NgayBatDau,
                NgayKetThuc = v.NgayKetThuc,
                HinhAnh = v.HinhAnh != null && v.HinhAnh.Length > 0 ? Convert.ToBase64String(v.HinhAnh) : null,
                DieuKien = v.DieuKien,
                TrangThai = v.TrangThai,
                LoaiVoucher = v.LoaiVoucher,
                GiaTriToiDa = v.GiaTriToiDa,
                Coupons = _context.Coupons
                    .Where(c => c.MaVoucher == v.MaVoucher)
                    .Select(c => new CouponView
                    {
                        ID = c.ID,
                        MaNhap = c.MaNhap,
                        TrangThai = c.TrangThai,
                        MaVoucher = c.MaVoucher,
                        MaNguoiDung = c.MaNguoiDung
                    })
                    .ToList()
            })
            .ToList();
    }

    public async Task<ValidateCouponResponse> ValidateCoupon(string code, int cartId)
    {
        try
        {
            // Lấy giỏ hàng và chi tiết giỏ hàng
            var cart = await _context.GioHangs
                .Include(c => c.ChiTietGioHangs)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .FirstOrDefaultAsync(c => c.MaGioHang == cartId);

            if (cart == null)
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Giỏ hàng không tồn tại"
                };
            }

            decimal originalAmount = cart.ChiTietGioHangs
                .Sum(item => item.ThanhTien ?? 0);

            var coupon = await _context.Coupons
                .Include(c => c.MaVoucherNavigation)
                .FirstOrDefaultAsync(c => c.MaNhap == code && (c.TrangThai == 0 || c.TrangThai == 2));

            if (coupon == null)
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Mã giảm giá không hợp lệ hoặc đã sử dụng"
                };
            }

            var voucher = coupon.MaVoucherNavigation;
            var now = DateTime.Now;

            if (voucher == null
                ||( voucher.TrangThai != 0 && voucher.TrangThai != 2)
                || voucher.NgayBatDau > now
                || voucher.NgayKetThuc < now
                || originalAmount < (voucher.DieuKien ?? 0))
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Mã giảm giá không hợp lệ, đã hết hạn, hoặc không đủ điều kiện"
                };
            }

            var shippingData = new Dictionary<string, decimal>
         {
            { "Hà Nội", (40000) },
    { "Hồ Chí Minh", (20000) },
    { "Hải Phòng", (45000) },
    { "Đà Nẵng", (30000) },
    { "Cần Thơ", (30000) },
    { "An Giang", (35000) },
    { "Bà Rịa - Vũng Tàu", (25000)},
    { "Bắc Giang", (45000 ) },
    { "Bắc Kạn", (50000) },
    { "Bạc Liêu", (35000) },
    { "Bắc Ninh", (40000 ) },
    { "Bến Tre", (30000) },
    { "Bình Định", (25000) },
    { "Bình Dương", (20000) },
    { "Bình Phước", (20000) },
    { "Bình Thuận", (25000) },
    { "Cà Mau", (35000) },
    { "Cao Bằng", (50000) },
    { "Đắk Lắk", (0) },
    { "Đắk Nông", (15000) },
    { "Điện Biên", (50000) },
    { "Đồng Nai", (20000) },
    { "Đồng Tháp", (30000) },
    { "Gia Lai", (15000) },
    { "Hà Giang", (50000) },
    { "Hà Nam", (45000 ) },
    { "Hà Tĩnh", (35000) },
    { "Hải Dương", (45000 ) },
    { "Hậu Giang", (35000) },
    { "Hòa Bình", (45000 ) },
    { "Hưng Yên", (40000 ) },
    { "Khánh Hòa", (25000) },
    { "Kiên Giang", (35000) },
    { "Kon Tum", (15000) },
    { "Lai Châu", (50000) },
    { "Lâm Đồng", (20000) },
    { "Lạng Sơn", (50000) },
    { "Lào Cai", (50000) },
    { "Long An", (30000) },
    { "Nam Định", (45000 ) },
    { "Nghệ An", (35000) },
    { "Ninh Bình", (45000 ) },
    { "Ninh Thuận", (25000) },
    { "Phú Thọ", (45000 ) },
    { "Phú Yên", (25000) },
    { "Quảng Bình", (35000) },
    { "Quảng Nam", (25000) },
    { "Quảng Ngãi", (25000) },
    { "Quảng Ninh", (50000) },
    { "Quảng Trị", (30000) },
    { "Sóc Trăng", (35000) },
    { "Sơn La", (50000) },
    { "Tây Ninh", (25000) },
    { "Thái Bình", (45000 ) },
    { "Thái Nguyên", (45000 ) },
    { "Thanh Hóa", (40000) },
    { "Thừa Thiên Huế", (30000) },
    { "Tiền Giang", (30000) },
    { "Trà Vinh", (30000) },
    { "Tuyên Quang", (50000) },
    { "Vĩnh Long", (30000) },
    { "Vĩnh Phúc", (45000 ) },
    { "Yên Bái", (50000) }
        };

            var address = await _context.DanhSachDiaChis
                .Where(a => a.MaNguoiDung == cart.MaNguoiDung && a.TrangThai == 1)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                address = await _context.DanhSachDiaChis
                    .Where(a => a.MaNguoiDung == cart.MaNguoiDung)
                    .FirstOrDefaultAsync();
            }

            string deliveryCity = address?.Tinh ?? "Không xác định";
            decimal shippingCost = shippingData.ContainsKey(deliveryCity) ? shippingData[deliveryCity] : 0;

            decimal discountAmount = 0;
            decimal finalAmount = originalAmount;

            switch (voucher.LoaiVoucher)
            {

                case 0:
                    decimal discountPercentage = (decimal)(voucher.GiaTri ?? 0);
                    discountAmount = originalAmount * (discountPercentage / 100);
                    decimal maxDiscount = voucher.GiaTriToiDa ?? 0;
                    if (finalAmount > maxDiscount)
                    {
                        return new ValidateCouponResponse
                        {
                            Success = false,
                            Message = "Mã giảm giá không thể áp dụng vì không đủ điều kiện",
                            DiscountAmount = 0,
                            FinalAmount = originalAmount,
                        };
                    }
                    finalAmount = originalAmount - discountAmount;
                    break;

                case 1:
                    discountAmount = (decimal)(voucher.GiaTri ?? 0);
                    decimal maxDiscountFixed = voucher.GiaTriToiDa ?? 0;
                    if (finalAmount > maxDiscountFixed)
                    {
                        return new ValidateCouponResponse
                        {
                            Success = false,
                            Message = "Mã giảm giá không thể áp dụng vì không đủ điều kiện",
                            DiscountAmount = 0,
                            FinalAmount = originalAmount,
                        };
                    }
                    finalAmount = originalAmount - discountAmount;
                    break;

                case 2: 
                    discountAmount = shippingCost; 
                    finalAmount = originalAmount;
                    break;

                default:
                    return new ValidateCouponResponse
                    {
                        Success = false,
                        Message = "Loại voucher không được hỗ trợ"
                    };
            }

            if (finalAmount < 0)
            {
                finalAmount = 0;
                discountAmount = originalAmount;
            }

            return new ValidateCouponResponse
            {
                Success = true,
                Message = "Mã giảm giá hợp lệ",
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
            };
        }
        catch (Exception ex)
        {
            return new ValidateCouponResponse
            {
                Success = false,
                Message = "Lỗi hệ thống khi xác thực mã giảm giá",
                DiscountAmount = 0,
                FinalAmount = 0,
            };
        }
    }

    public async Task<VoucherView> CreateVoucher(VoucherCreate voucher)
    {
        var newVoucher = new Voucher
        {
            TenVoucher = voucher.TenVoucher,
            GiaTri = voucher.GiaTri,
            MoTa = voucher.MoTa,
            NgayBatDau = voucher.NgayBatDau,
            NgayKetThuc = voucher.NgayKetThuc,
            DieuKien = voucher.DieuKien,
            LoaiVoucher = voucher.LoaiVoucher,
            GiaTriToiDa = voucher.GiaTriToiDa,
            TrangThai = 0,
            HinhAnh = !string.IsNullOrEmpty(voucher.HinhAnh) ? Convert.FromBase64String(voucher.HinhAnh) : null
        };

        _context.Vouchers.Add(newVoucher);
        await _context.SaveChangesAsync();

        // Tạo coupon ngẫu nhiên
        Random random = new Random();
        for (int i = 0; i < 5; i++)
        {
            string maNhap = "VC" + random.Next(11111, 99999);
            var coupon = new Coupon
            {
                MaNhap = maNhap,
                TrangThai = 0,
                MaVoucher = newVoucher.MaVoucher,
                MaNguoiDung = null

            };
            _context.Coupons.Add(coupon);
        }
        await _context.SaveChangesAsync();

        // Trả về dữ liệu với base64 string
        return new VoucherView
        {
            MaVoucher = newVoucher.MaVoucher,
            TenVoucher = newVoucher.TenVoucher,
            GiaTri = newVoucher.GiaTri,
            MoTa = newVoucher.MoTa,
            NgayBatDau = newVoucher.NgayBatDau,
            NgayKetThuc = newVoucher.NgayKetThuc,
            HinhAnh = newVoucher.HinhAnh != null ? Convert.ToBase64String(newVoucher.HinhAnh) : null, // Sửa lỗi cú pháp và logic
            DieuKien = newVoucher.DieuKien,
            TrangThai = newVoucher.TrangThai,
            LoaiVoucher = newVoucher.LoaiVoucher,

            GiaTriToiDa = newVoucher.GiaTriToiDa,
            Coupons = _context.Coupons
                .Where(c => c.MaVoucher == newVoucher.MaVoucher)
                .Select(c => new CouponView
                {
                    ID = c.ID,
                    MaNhap = c.MaNhap,
                    TrangThai = c.TrangThai,
                    MaVoucher = c.MaVoucher,
                    MaNguoiDung = c.MaNguoiDung
                })
                .ToList()
        };
    }

    public async Task<VoucherView> EditVoucher(VoucherEdit voucher)
    {
        var existingVoucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.MaVoucher == voucher.MaVoucher);

        if (existingVoucher == null)
        {
            throw new Exception("Không tìm thấy voucher với mã được cung cấp");
        }

        existingVoucher.TenVoucher = voucher.TenVoucher;
        existingVoucher.GiaTri = voucher.GiaTri.Value;
        existingVoucher.MoTa = voucher.MoTa;
        existingVoucher.NgayBatDau = voucher.NgayBatDau.Value;
        existingVoucher.NgayKetThuc = voucher.NgayKetThuc.Value;
        existingVoucher.DieuKien = voucher.DieuKien.Value;
        existingVoucher.TrangThai = voucher.TrangThai.Value;
        existingVoucher.LoaiVoucher = voucher.LoaiVoucher.Value;
        existingVoucher.GiaTriToiDa = voucher.GiaTriToiDa.Value;


        // Xử lý hình ảnh
        if (!string.IsNullOrEmpty(voucher.HinhAnh))
        {
            existingVoucher.HinhAnh = Convert.FromBase64String(voucher.HinhAnh);
        }

        await _context.SaveChangesAsync();

        return new VoucherView
        {
            MaVoucher = existingVoucher.MaVoucher,
            TenVoucher = existingVoucher.TenVoucher,
            GiaTri = existingVoucher.GiaTri,
            MoTa = existingVoucher.MoTa,
            NgayBatDau = existingVoucher.NgayBatDau,
            NgayKetThuc = existingVoucher.NgayKetThuc,
            HinhAnh = existingVoucher.HinhAnh != null ? Convert.ToBase64String(existingVoucher.HinhAnh) : null,
            DieuKien = existingVoucher.DieuKien,
            TrangThai = existingVoucher.TrangThai,
            LoaiVoucher = existingVoucher.LoaiVoucher,

            GiaTriToiDa = existingVoucher.GiaTriToiDa,
        };
    }

    public async Task<bool> DeleteVoucher(int maVoucher)
    {
        var voucherToDelete = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.MaVoucher == maVoucher);

        if (voucherToDelete == null)
        {
            return false;
        }

        var relatedCoupons = await _context.Coupons
            .Where(c => c.MaVoucher == maVoucher)
            .ToListAsync();

        _context.Coupons.RemoveRange(relatedCoupons);
        _context.Vouchers.Remove(voucherToDelete);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateCoupon(int couponId, string maNguoiDung)
    {
        try
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.ID == couponId && c.TrangThai == 0);

            if (coupon == null)
            {
                return false; // Coupon không tồn tại hoặc đã được sử dụng
            }

            coupon.MaNguoiDung = maNguoiDung;
            coupon.TrangThai = 2; // Đổi trạng thái thành 2 (đã lưu cho người dùng)

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}