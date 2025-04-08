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
            // Đường link ngắn của ảnh mặc định
            string defaultImageUrl = "data:image/jpeg;base64,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"; // Có thể thay bằng link ảnh khác

            // Lấy danh sách bình luận từ bảng BinhLuans
            var binhLuans = await _context.BinhLuans
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

            // Duyệt qua từng bình luận để lấy thông tin bổ sung
            foreach (var binhLuan in binhLuans)
            {
                // Lấy thông tin người dùng
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(nd => nd.MaNguoiDung == binhLuan.MaNguoiDung);

                // Gán HoTen
                binhLuan.HoTen = nguoiDung?.HoTen;

                // Gán HinhAnh
                binhLuan.HinhAnh = nguoiDung != null && nguoiDung.HinhAnh != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(nguoiDung.HinhAnh)}"
                    : defaultImageUrl;

                // Lấy thông tin sản phẩm (so sánh 6 ký tự đầu của MaSanPham)
                if (!string.IsNullOrEmpty(binhLuan.MaSanPham) && binhLuan.MaSanPham.Length >= 6)
                {
                    string maSanPhamShort = binhLuan.MaSanPham.Substring(0, 6); // Lấy 6 ký tự đầu
                    var sanPham = await _context.SanPhams
                        .FirstOrDefaultAsync(sp => sp.MaSanPham.StartsWith(maSanPhamShort));
                    binhLuan.TenSanPham = sanPham?.TenSanPham;
                }
                else
                {
                    binhLuan.TenSanPham = null; // Nếu MaSanPham không đủ dài, gán null
                }
            }

            return binhLuans;
        }

        public async Task<BinhLuanView> AddBinhLuan(BinhLuanCreate binhLuan)
        {
            // Tạo một đối tượng BinhLuan từ BinhLuanCreate
            var newBinhLuan = new BinhLuan
            {
                MaSanPham = binhLuan.MaSanPham,
                MaNguoiDung = binhLuan.MaNguoiDung,
                NoiDungBinhLuan = binhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = binhLuan.SoTimBinhLuan ?? 0, // Giá trị mặc định nếu null
                DanhGia = binhLuan.DanhGia,
                TrangThai = 0, // Giá trị mặc định nếu null
                NgayBinhLuan = binhLuan.NgayBinhLuan ?? DateTime.Now // Gán ngày hiện tại nếu null
            };

            _context.BinhLuans.Add(newBinhLuan);
            await _context.SaveChangesAsync();

            // Lấy thông tin bổ sung
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.MaNguoiDung == binhLuan.MaNguoiDung);
            string tenSanPham = null;
            if (!string.IsNullOrEmpty(binhLuan.MaSanPham) && binhLuan.MaSanPham.Length >= 6)
            {
                string maSanPhamShort = binhLuan.MaSanPham.Substring(0, 6);
                var sanPham = await _context.SanPhams
                    .FirstOrDefaultAsync(sp => sp.MaSanPham.StartsWith(maSanPhamShort));
                tenSanPham = sanPham?.TenSanPham;
            }

            // Trả về một BinhLuanView từ dữ liệu vừa thêm
            return new BinhLuanView
            {
                MaBinhLuan = newBinhLuan.MaBinhLuan,
                MaSanPham = newBinhLuan.MaSanPham,
                TenSanPham = tenSanPham,
                MaNguoiDung = newBinhLuan.MaNguoiDung,
                HoTen = nguoiDung?.HoTen,
                NoiDungBinhLuan = newBinhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = newBinhLuan.SoTimBinhLuan,
                DanhGia = newBinhLuan.DanhGia,
                TrangThai = 0,
                NgayBinhLuan = newBinhLuan.NgayBinhLuan,
                HinhAnh = nguoiDung != null && nguoiDung.HinhAnh != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(nguoiDung.HinhAnh)}"
                    : "data:image/jpeg;base64,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"
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

            // Lấy thông tin bổ sung
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.MaNguoiDung == binhLuan.MaNguoiDung);
            string tenSanPham = null;
            if (!string.IsNullOrEmpty(binhLuan.MaSanPham) && binhLuan.MaSanPham.Length >= 6)
            {
                string maSanPhamShort = binhLuan.MaSanPham.Substring(0, 6);
                var sanPham = await _context.SanPhams
                    .FirstOrDefaultAsync(sp => sp.MaSanPham.StartsWith(maSanPhamShort));
                tenSanPham = sanPham?.TenSanPham;
            }

            // Trả về một BinhLuanView từ dữ liệu vừa cập nhật
            return new BinhLuanView
            {
                MaBinhLuan = existingBinhLuan.MaBinhLuan,
                MaSanPham = existingBinhLuan.MaSanPham,
                TenSanPham = tenSanPham,
                MaNguoiDung = existingBinhLuan.MaNguoiDung,
                HoTen = nguoiDung?.HoTen,
                NoiDungBinhLuan = existingBinhLuan.NoiDungBinhLuan,
                SoTimBinhLuan = existingBinhLuan.SoTimBinhLuan,
                DanhGia = existingBinhLuan.DanhGia,
                TrangThai = existingBinhLuan.TrangThai,
                NgayBinhLuan = existingBinhLuan.NgayBinhLuan,
                HinhAnh = nguoiDung != null && nguoiDung.HinhAnh != null
                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(nguoiDung.HinhAnh)}"
                    : "https://via.placeholder.com/50"
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

        public async Task<bool> LikeBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.SoTimBinhLuan = (binhLuan.SoTimBinhLuan ?? 0) + 1; // Tăng số lượt thích
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeBinhLuan(int maBinhLuan)
        {
            var binhLuan = await _context.BinhLuans
                .FirstOrDefaultAsync(bl => bl.MaBinhLuan == maBinhLuan);

            if (binhLuan == null)
            {
                return false;
            }

            binhLuan.SoTimBinhLuan = Math.Max(0, (binhLuan.SoTimBinhLuan ?? 0) - 1); // Giảm số lượt thích, không âm
            await _context.SaveChangesAsync();
            return true;
        }
    }

  
   
}