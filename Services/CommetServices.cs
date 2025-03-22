using UltraStrore.Data;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;

namespace UltraStrore.Services
{
    public class CommetServices : ICommetServices
    {
        private readonly ApplicationDbContext _context;

        public CommetServices(ApplicationDbContext context)
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
                    TenSanPham = bl .TenSanPham,
                    MaNguoiDung = bl.MaNguoiDung,
                    HoTen = bl.HoTen,
                    NoiDungBinhLuan = bl.NoiDungBinhLuan,
                    SoTimBinhLuan = bl.SoTimBinhLuan,
                    DanhGia = bl.DanhGia,
                    TrangThai = bl.TrangThai,
                    NgayBinhLuan = bl.NgayBinhLuan
                })
                .ToListAsync();

            return result;
        }

        public async Task<BinhLuanView> AddBinhLuan(BinhLuanCreate binhLuan)
        {
            // Tạo một đối tượng BinhLuan từ BinhLuanCreate
            var newBinhLuan = new BinhLuan
            {
                MaSanPham = binhLuan.MaSanPham,
                TenSanPham = binhLuan.TenSanPham,
                MaNguoiDung = binhLuan.MaNguoiDung,
                HoTen = binhLuan.HoTen,
                NoiDungBinhLuan = binhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = binhLuan.SoTimBinhLuan ?? 0, // Giá trị mặc định nếu null
                DanhGia = binhLuan.DanhGia,
                TrangThai =  0, // Giá trị mặc định nếu null
                NgayBinhLuan = binhLuan.NgayBinhLuan ?? DateTime.Now // Gán ngày hiện tại nếu null
            };

            _context.BinhLuans.Add(newBinhLuan);
            await _context.SaveChangesAsync();

            // Trả về một BinhLuanView từ dữ liệu vừa thêm
            return new BinhLuanView
            {
                MaBinhLuan = newBinhLuan.MaBinhLuan,
                MaSanPham = newBinhLuan.MaSanPham,
                TenSanPham = newBinhLuan.TenSanPham,
                MaNguoiDung = newBinhLuan.MaNguoiDung,
                HoTen = newBinhLuan.HoTen,
                NoiDungBinhLuan = newBinhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = newBinhLuan.SoTimBinhLuan,
                DanhGia = newBinhLuan.DanhGia,
                TrangThai = 0,
                NgayBinhLuan = newBinhLuan.NgayBinhLuan
            };
        }

        public async Task<BinhLuanView> UpdateBinhLuan(int maBinhLuan, BinhLuanEdit binhLuan)
        {
            var existingBinhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (existingBinhLuan == null)
            {
                return null; // Hoặc throw exception tùy yêu cầu
            }

            // Cập nhật các thuộc tính
            existingBinhLuan.MaSanPham = binhLuan.MaSanPham;
            existingBinhLuan.MaNguoiDung = binhLuan.MaNguoiDung;
            existingBinhLuan.NoiDungBinhLuan = binhLuan.NoiDungBinhLuan;
            existingBinhLuan.SoTimBinhLuan = binhLuan.SoTimBinhLuan;
            existingBinhLuan.DanhGia = binhLuan.DanhGia;
            existingBinhLuan.NgayBinhLuan = binhLuan.NgayBinhLuan;

            await _context.SaveChangesAsync();

            // Trả về một BinhLuanView từ dữ liệu vừa cập nhật
            return new BinhLuanView
            {
                MaBinhLuan = existingBinhLuan.MaBinhLuan,
                MaSanPham = existingBinhLuan.MaSanPham,
                MaNguoiDung = existingBinhLuan.MaNguoiDung,
                NoiDungBinhLuan = existingBinhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = existingBinhLuan.SoTimBinhLuan,
                DanhGia = existingBinhLuan.DanhGia,
                TrangThai = existingBinhLuan.TrangThai,
                NgayBinhLuan = existingBinhLuan.NgayBinhLuan
            };
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
            await _context.SaveChangesAsync();
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