using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        var yeuThich = new YeuThich
        {
            MaYeuThich = yeuThichCreate.MaYeuThich,
            MaSanPham = yeuThichCreate.MaSanPham,
            TenSanPham = yeuThichCreate.TenSanPham,
            MaNguoiDung = yeuThichCreate.MaNguoiDung,
            HoTen = yeuThichCreate.HoTen,
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
