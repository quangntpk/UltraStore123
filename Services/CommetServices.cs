using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UltraStrore.Repository;
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

        // Get list of comments with the relevant data
        public async Task<List<BinhLuanView>> ListBinhLuan()
        {
            if (_context.BinhLuans == null)
                throw new InvalidOperationException("Không thể truy cập danh sách bình luận.");

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

            return result ?? new List<BinhLuanView>(); // Đảm bảo không trả về null
        }

        // Add a new comment
        public async Task<BinhLuanView> AddBinhLuan(CommentCreate binhLuan)
        {
            if (binhLuan == null || string.IsNullOrEmpty(binhLuan.NoiDungBinhLuan))
                throw new ArgumentException("Dữ liệu bình luận không hợp lệ.");

            var newBinhLuan = new BinhLuan
            {
                MaSanPham = binhLuan.MaSanPham,
                MaNguoiDung = binhLuan.MaNguoiDung,
                NoiDungBinhLuan = binhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = binhLuan.SoTimBinhLuan ?? 0, // Default value
                DanhGia = binhLuan.DanhGia,
                NgayBinhLuan = binhLuan.NgayBinhLuan ?? DateTime.Now, // Default to current time
                TrangThai = 0
            };

            // Không cần gán MaBinhLuan, vì nó sẽ tự động tăng trong cơ sở dữ liệu
            _context.BinhLuans.Add(newBinhLuan);
            await _context.SaveChangesAsync();

            return new BinhLuanView
            {
                MaBinhLuan = newBinhLuan.MaBinhLuan,  // Sau khi lưu vào DB, mã sẽ tự động có giá trị
                MaSanPham = newBinhLuan.MaSanPham,
                MaNguoiDung = newBinhLuan.MaNguoiDung,
                NoiDungBinhLuan = newBinhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = newBinhLuan.SoTimBinhLuan,
                DanhGia = newBinhLuan.DanhGia,
                TrangThai = newBinhLuan.TrangThai,
                NgayBinhLuan = newBinhLuan.NgayBinhLuan
            };
        }


        // Update an existing comment
        public async Task<BinhLuanView> UpdateBinhLuan(int maBinhLuan, CommentEdit binhLuan) // Sửa thành CommentEdit
        {
            if (binhLuan == null)
                throw new ArgumentException("Dữ liệu bình luận không hợp lệ.");

            var existingBinhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (existingBinhLuan == null)
                throw new KeyNotFoundException("Không tìm thấy bình luận để cập nhật.");

            existingBinhLuan.NoiDungBinhLuan = binhLuan.NoiDungBinhLuan ?? existingBinhLuan.NoiDungBinhLuan; // Chỉ cập nhật nếu có giá trị
            existingBinhLuan.SoTimBinhLuan = binhLuan.SoTimBinhLuan ?? existingBinhLuan.SoTimBinhLuan;
            existingBinhLuan.DanhGia = binhLuan.DanhGia ?? existingBinhLuan.DanhGia;

            await _context.SaveChangesAsync();

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

        // Delete a comment
        public async Task<bool> DeleteBinhLuan(int maBinhLuan)
        {
            var binhLuanToRemove = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuanToRemove == null)
                throw new KeyNotFoundException("Không tìm thấy bình luận để xóa.");

            _context.BinhLuans.Remove(binhLuanToRemove);
            await _context.SaveChangesAsync();
            return true;
        }

        // Approve a comment
        public async Task<bool> ApproveBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false; // If comment doesn't exist, return false
            }

            binhLuan.TrangThai = 1; // Set status to "Approved"
            await _context.SaveChangesAsync();
            return true;
        }

        // Unapprove a comment
        public async Task<bool> UnapproveBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false; // If comment doesn't exist, return false
            }

            binhLuan.TrangThai = 0; // Set status to "Not Approved"
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
