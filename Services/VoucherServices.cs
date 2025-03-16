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
                SoLuong = v.SoLuong,
                TrangThai = v.TrangThai,
                Coupons = _context.Coupons
                    .Where(c => c.MaVoucher == v.MaVoucher)
                    .Select(c => new CouponView
                    {
                        ID = c.ID,
                        MaNhap = c.MaNhap,
                        TrangThai = c.TrangThai,
                        MaVoucher = c.MaVoucher
                    })
                    .ToList()
            })
            .ToList();
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
            SoLuong = voucher.SoLuong,
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
                MaVoucher = newVoucher.MaVoucher
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
            SoLuong = newVoucher.SoLuong,
            TrangThai = newVoucher.TrangThai,
            Coupons = _context.Coupons
                .Where(c => c.MaVoucher == newVoucher.MaVoucher)
                .Select(c => new CouponView
                {
                    ID = c.ID,
                    MaNhap = c.MaNhap,
                    TrangThai = c.TrangThai,
                    MaVoucher = c.MaVoucher
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
        existingVoucher.SoLuong = voucher.SoLuong.Value;
        existingVoucher.TrangThai = voucher.TrangThai.Value;

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
            SoLuong = existingVoucher.SoLuong,
            TrangThai = existingVoucher.TrangThai,
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
}