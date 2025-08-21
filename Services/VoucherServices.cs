using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Services;
using UltraStrore.Utils;


public class VoucherServices : IVoucherServices
{
    private readonly ApplicationDbContext _context;
    private readonly IGHNService _gHNService;
    public VoucherServices(ApplicationDbContext context, IGHNService gHNService)
    {
        _context = context;
        _gHNService = gHNService;
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
            // Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(code))
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Mã giảm giá không được để trống"
                };
            }
            if (cartId <= 0)
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "ID giỏ hàng không hợp lệ"
                };
            }

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

            if (cart.ChiTietGioHangs == null || !cart.ChiTietGioHangs.Any())
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Giỏ hàng không có sản phẩm"
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

            if (voucher == null)
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Mã giảm giá không hợp lệ"
                };
            }

            if (voucher.TrangThai != 0)
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Mã giảm giá đã được sử dụng"
                };
            }

            if (voucher.NgayBatDau > now)
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Mã giảm giá chưa bắt đầu"
                };
            }

            if (voucher.NgayKetThuc < now)
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Mã giảm giá đã hết hạn"
                };
            }

            if (originalAmount < (voucher.DieuKien ?? 0))
            {
                return new ValidateCouponResponse
                {
                    Success = false,
                    Message = "Tổng tiền không đủ điều kiện để sử dụng mã giảm giá"
                };
            }

            var address = await _context.DanhSachDiaChis
                .Where(a => a.MaNguoiDung == cart.MaNguoiDung && a.TrangThai == 1)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                address = await _context.DanhSachDiaChis
                    .Where(a => a.MaNguoiDung == cart.MaNguoiDung)
                    .FirstOrDefaultAsync();
            }

            decimal shippingCost = 0;
            if (address != null)
            {
                try
                {
                    var districtId = await GetDistrictIdFromName(address.QuanHuyen, address.Tinh);
                    var wardCode = await GetWardCodeFromName(address.PhuongXa, address.QuanHuyen, address.Tinh);

                    if (districtId == 0 || string.IsNullOrEmpty(wardCode))
                    {
                        return new ValidateCouponResponse
                        {
                            Success = false,
                            Message = "Không thể xác định mã quận/huyện hoặc phường/xã"
                        };
                    }

                    var shippingFeeRequest = new ShippingFeeRequest
                    {
                        service_type_id = 2,
                        to_district_id = districtId,
                        to_ward_code = wardCode,
                        weight = 1000,
                        length = 15,
                        width = 15,
                        height = 15,
                        insurance_value = 0,
                        coupon = null
                    };

                    var shippingFeeResponse = await _gHNService.GetShippingFee(shippingFeeRequest);
                    shippingCost = shippingFeeResponse.total ?? 0;
                }
                catch (Exception ex)
                {
                    return new ValidateCouponResponse
                    {
                        Success = false,
                        Message = "Lỗi khi tính phí vận chuyển: " + ex.Message
                    };
                }
            }

            decimal discountAmount = 0;
            decimal finalAmount = originalAmount;

            switch (voucher.LoaiVoucher)
            {
                case 0:
                    decimal discountPercentage = (decimal)(voucher.GiaTri ?? 0);
                    discountAmount = originalAmount * (discountPercentage / 100);
                    decimal maxDiscount = voucher.GiaTriToiDa ?? 0;
                    if (discountAmount > maxDiscount)
                    {
                        discountAmount = maxDiscount;
                    }
                    finalAmount = originalAmount - discountAmount;
                    break;

                case 1:
                    discountAmount = (decimal)(voucher.GiaTri ?? 0);
                    decimal maxDiscountFixed = voucher.GiaTriToiDa ?? 0;
                    if (discountAmount > maxDiscountFixed)
                    {
                        discountAmount = maxDiscountFixed;
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
                FinalAmount = finalAmount
            };
        }
        catch (Exception ex)
        {
            return new ValidateCouponResponse
            {
                Success = false,
                Message = "Lỗi hệ thống khi xác thực mã giảm giá: " + ex.Message,
                DiscountAmount = 0,
                FinalAmount = 0
            };
        }
    }
    private async Task<int> GetDistrictIdFromName(string districtName, string provinceName)
    {
        var provinces = await _gHNService.GetProvinces();
        var province = provinces.FirstOrDefault(p => p.ProvinceName.ToLower().Contains(provinceName.ToLower()));
        if (province == null) return 0;

        var districts = await _gHNService.GetDistricts(province.ProvinceID);
        return districts.FirstOrDefault(d => d.DistrictName.ToLower().Contains(districtName.ToLower()))?.DistrictID ?? 0;
    }


    private async Task<string> GetWardCodeFromName(string wardName, string districtName, string provinceName)
    {
        var districtId = await GetDistrictIdFromName(districtName, provinceName);
        if (districtId == 0) return "";

        var wards = await _gHNService.GetWards(districtId);
        return wards.FirstOrDefault(w => w.WardName.ToLower().Contains(wardName.ToLower()))?.WardCode ?? "";
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