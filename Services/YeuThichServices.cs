using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

public class YeuThichServices : IYeuThichServices
{
    private readonly ApplicationDbContext _context;

    public YeuThichServices(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<YeuThichView> GetAllYeuThichs()
    {
        return _context.YeuThichs
            .Select(y => new YeuThichView
            {
                MaYeuThich = y.MaYeuThich,
                MaSanPham = y.MaSanPham,
                TenSanPham = y.TenSanPham,
                MaNguoiDung = y.MaNguoiDung,
                HoTen = y.HoTen,
                NgayYeuThich = y.NgayYeuThich
            })
            .ToList();
    }

    public async Task<YeuThichView> CreateYeuThich(YeuThichCreate yeuThichCreate)
    {
        // Take the first 6 characters of MaSanPham from yeuThichCreate
        string maSanPhamPrefix = yeuThichCreate.MaSanPham.Length >= 6
            ? yeuThichCreate.MaSanPham.Substring(0, 6)
            : yeuThichCreate.MaSanPham;

        // Fetch TenSanPham by matching the first 6 characters of MaSanPham
        var sanPham = await _context.SanPhams
            .Where(sp => EF.Functions.Like(sp.MaSanPham, $"{maSanPhamPrefix}%"))
            .Select(sp => sp.TenSanPham)
            .FirstOrDefaultAsync();

        // Fetch HoTen from NguoiDung table
        var nguoiDung = await _context.NguoiDungs
            .Where(nd => nd.MaNguoiDung == yeuThichCreate.MaNguoiDung)
            .Select(nd => nd.HoTen)
            .FirstOrDefaultAsync();

        // Check if the product or user exists
        if (sanPham == null || nguoiDung == null)
        {
            throw new Exception("Product or User not found.");
        }

        var yeuThich = new YeuThich
        {
            MaYeuThich = yeuThichCreate.MaYeuThich,
            MaSanPham = yeuThichCreate.MaSanPham,
            TenSanPham = sanPham, // Assign fetched product name
            MaNguoiDung = yeuThichCreate.MaNguoiDung,
            HoTen = nguoiDung, // Assign fetched full name
            NgayYeuThich = yeuThichCreate.NgayYeuThich
        };

        _context.YeuThichs.Add(yeuThich);
        await _context.SaveChangesAsync();

        return new YeuThichView
        {
            MaYeuThich = yeuThich.MaYeuThich,
            MaSanPham = yeuThich.MaSanPham,
            TenSanPham = yeuThich.TenSanPham,
            MaNguoiDung = yeuThich.MaNguoiDung,
            HoTen = yeuThich.HoTen,
            NgayYeuThich = yeuThich.NgayYeuThich
        };
    }

    public async Task<bool> DeleteYeuThich(int maYeuThich)
    {
        var yeuThich = await _context.YeuThichs.FindAsync(maYeuThich);
        if (yeuThich == null)
            return false;

        _context.YeuThichs.Remove(yeuThich);
        await _context.SaveChangesAsync();
        return true;
    }
}