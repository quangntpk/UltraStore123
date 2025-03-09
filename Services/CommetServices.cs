using UltraStrore.Data;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UltraStrore.Services
{
    public class CommetServices : ICommetServices
    {
        private readonly ApplicationDbContext _context; // Khai báo DbContext

        // Constructor để inject DbContext
        public CommetServices(ApplicationDbContext context) // Sửa lỗi cú pháp: bỏ dấu ; và đổi tên tham số
        {
            _context = context;
        }

        public async Task<List<BinhLuanView>> ListBinhLuan()
        {
            var result = await _context.BinhLuans
                .Select(bl => new BinhLuanView
                {
                    MaBinhLuan = bl.MaBinhLuan,
                    MaSanPham = bl.MaSanPham,
                    MaNguoiDung = bl.MaNguoiDung,
                    NoiDungBinhLuan = bl.NoiDungBinhLuan,
                    SoTimBinhLuan = bl.SoTimBinhLuan,
                    DanhGia = bl.DanhGia,
                    TrangThai = bl.TrangThai,
                    NgayBinhLuan = bl.NgayBinhLuan
                })
                .ToListAsync();

            return result;
        }

        public async Task<BinhLuan> AddBinhLuan(BinhLuan binhLuan)
        {
            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            return binhLuan;
        }

        public async Task<BinhLuan> UpdateBinhLuan(int maBinhLuan, BinhLuan binhLuan)
        {
            var existingBinhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

          

            existingBinhLuan.MaSanPham = binhLuan.MaSanPham;
            existingBinhLuan.MaNguoiDung = binhLuan.MaNguoiDung;
            existingBinhLuan.NoiDungBinhLuan = binhLuan.NoiDungBinhLuan;
            existingBinhLuan.SoTimBinhLuan = binhLuan.SoTimBinhLuan;
            existingBinhLuan.DanhGia = binhLuan.DanhGia;
            existingBinhLuan.NgayBinhLuan = binhLuan.NgayBinhLuan;

            await _context.SaveChangesAsync(); // Lưu thay đổi
            return existingBinhLuan;
        }

        public async Task<bool> DeleteBinhLuan(int maBinhLuan)
        {
            var binhLuanToRemove = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuanToRemove == null)
            {
                return false;
            }

            _context.BinhLuans.Remove(binhLuanToRemove);
            await _context.SaveChangesAsync(); // Lưu thay đổi
            return true;
        }


        public async Task<bool> ApproveBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.TrangThai = 1; // Cập nhật trạng thái thành "Đã Duyệt"
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> UnapproveBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.TrangThai = 0; // Cập nhật trạng thái thành "Chưa Duyệt"
            await _context.SaveChangesAsync();
            return true;
        }
    }
}