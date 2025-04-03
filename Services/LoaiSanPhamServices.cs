using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class LoaiSanPhamServices : ILoaiSanPhamServices
    {
        private readonly ApplicationDbContext _context;

        public LoaiSanPhamServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoaiSanPhamView> CreateLoaiSanPhamAsync(LoaiSanPhamCreate model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var newLoaiSanPham = new LoaiSanPham
            {
                TenLoaiSanPham = model.TenLoaiSanPham,
                KiHieu = model.KiHieu,
                HinhAnh = model.HinhAnh
            };

            _context.LoaiSanPhams.Add(newLoaiSanPham);
            await _context.SaveChangesAsync();

            return new LoaiSanPhamView
            {
                MaLoaiSanPham = newLoaiSanPham.MaLoaiSanPham,
                TenLoaiSanPham = newLoaiSanPham.TenLoaiSanPham,
                KiHieu = newLoaiSanPham.KiHieu,
                HinhAnh = newLoaiSanPham.HinhAnh
            };
        }

        public async Task<List<LoaiSanPhamView>> GetAllLoaiSanPhamAsync()
        {
            var list = await _context.LoaiSanPhams.ToListAsync();
            return list.Select(l => new LoaiSanPhamView
            {
                MaLoaiSanPham = l.MaLoaiSanPham,
                TenLoaiSanPham = l.TenLoaiSanPham,
                KiHieu = l.KiHieu,
                HinhAnh = l.HinhAnh
            }).ToList();
        }

        public async Task<LoaiSanPhamView> GetLoaiSanPhamAsync(int maLoaiSanPham)
        {
            var loaiSanPham = await _context.LoaiSanPhams
                .FirstOrDefaultAsync(l => l.MaLoaiSanPham == maLoaiSanPham);
            if (loaiSanPham == null)
                throw new Exception("Loại sản phẩm không tồn tại.");

            return new LoaiSanPhamView
            {
                MaLoaiSanPham = loaiSanPham.MaLoaiSanPham,
                TenLoaiSanPham = loaiSanPham.TenLoaiSanPham,
                KiHieu = loaiSanPham.KiHieu,
                HinhAnh = loaiSanPham.HinhAnh
            };
        }

        public async Task<LoaiSanPhamView> UpdateLoaiSanPhamAsync(LoaiSanPhamEdit model)
        {
            var loaiSanPham = await _context.LoaiSanPhams
                .FirstOrDefaultAsync(l => l.MaLoaiSanPham == model.MaLoaiSanPham);
            if (loaiSanPham == null)
                throw new Exception("Loại sản phẩm không tồn tại.");

            loaiSanPham.TenLoaiSanPham = model.TenLoaiSanPham;
            loaiSanPham.KiHieu = model.KiHieu;
            loaiSanPham.HinhAnh = model.HinhAnh;

            await _context.SaveChangesAsync();

            return new LoaiSanPhamView
            {
                MaLoaiSanPham = loaiSanPham.MaLoaiSanPham,
                TenLoaiSanPham = loaiSanPham.TenLoaiSanPham,
                KiHieu = loaiSanPham.KiHieu,
                HinhAnh = loaiSanPham.HinhAnh
            };
        }

        public async Task<bool> DeleteLoaiSanPhamAsync(int maLoaiSanPham)
        {
            var loaiSanPham = await _context.LoaiSanPhams
                .FirstOrDefaultAsync(l => l.MaLoaiSanPham == maLoaiSanPham);
            if (loaiSanPham == null)
                return false;

            _context.LoaiSanPhams.Remove(loaiSanPham);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SanPhamView>> GetSanPhamByLoaiAsync(int maLoaiSanPham)
        {
            var sanPhams = await _context.SanPhams
                .Where(s => s.MaLoaiSanPham == maLoaiSanPham)
                .ToListAsync();
            return sanPhams.Select(s => new SanPhamView
            {
                ID = s.MaSanPham,
                Name = s.TenSanPham
            }).ToList();
        }

        public async Task<List<LoaiSanPhamView>> SearchLoaiSanPhamAsync(string? tenLoai, string? kiHieu)
        {
            var query = _context.LoaiSanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(tenLoai))
            {
                query = query.Where(l => l.TenLoaiSanPham.Contains(tenLoai));
            }

            if (!string.IsNullOrEmpty(kiHieu))
            {
                query = query.Where(l => l.KiHieu.Contains(kiHieu));
            }

            var list = await query.ToListAsync();
            return list.Select(l => new LoaiSanPhamView
            {
                MaLoaiSanPham = l.MaLoaiSanPham,
                TenLoaiSanPham = l.TenLoaiSanPham,
                KiHieu = l.KiHieu,
                HinhAnh = l.HinhAnh
            }).ToList();
        }
    }
}