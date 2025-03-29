using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class GiaoDienServices : IGiaoDienServices
    {
        private readonly ApplicationDbContext _context;

        public GiaoDienServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GiaoDienView> CreateGiaoDienAsync(GiaoDienCreate model)
        {
            var newGiaoDien = new GiaoDien
            {
                TenGiaoDien = model.TenGiaoDien,
                Logo = model.Logo,
                Slider1 = model.Slider1,
                Slider2 = model.Slider2,
                Slider3 = model.Slider3,
                Slider4 = model.Slider4,
                Avt = model.Avt,
                NgayTao = DateTime.Now,
                TrangThai = 0
            };

            _context.GiaoDiens.Add(newGiaoDien);
            await _context.SaveChangesAsync();

            return new GiaoDienView
            {
                MaGiaoDien = newGiaoDien.MaGiaoDien,
                TenGiaoDien = newGiaoDien.TenGiaoDien,
                Logo = newGiaoDien.Logo,
                Slider1 = newGiaoDien.Slider1,
                Slider2 = newGiaoDien.Slider2,
                Slider3 = newGiaoDien.Slider3,
                Slider4 = newGiaoDien.Slider4,
                Avt = newGiaoDien.Avt,
                NgayTao = newGiaoDien.NgayTao,
                TrangThai = newGiaoDien.TrangThai
            };
        }

        public async Task<List<GiaoDienView>> GetAllGiaoDienAsync()
        {
            var list = await _context.GiaoDiens.ToListAsync();
            return list.Select(g => new GiaoDienView
            {
                MaGiaoDien = g.MaGiaoDien,
                TenGiaoDien = g.TenGiaoDien,
                Logo = g.Logo,
                Slider1 = g.Slider1,
                Slider2 = g.Slider2,
                Slider3 = g.Slider3,
                Slider4 = g.Slider4,
                Avt = g.Avt,
                NgayTao = g.NgayTao,
                TrangThai = g.TrangThai
            }).ToList();
        }

        public async Task<GiaoDienView> GetGiaoDienAsync(int maGiaoDien)
        {
            var giaoDien = await _context.GiaoDiens
                .FirstOrDefaultAsync(g => g.MaGiaoDien == maGiaoDien);
            if (giaoDien == null)
                throw new Exception("Giao diện không tồn tại.");

            return new GiaoDienView
            {
                MaGiaoDien = giaoDien.MaGiaoDien,
                TenGiaoDien = giaoDien.TenGiaoDien,
                Logo = giaoDien.Logo,
                Slider1 = giaoDien.Slider1,
                Slider2 = giaoDien.Slider2,
                Slider3 = giaoDien.Slider3,
                Slider4 = giaoDien.Slider4,
                Avt = giaoDien.Avt,
                NgayTao = giaoDien.NgayTao,
                TrangThai = giaoDien.TrangThai
            };
        }

        public async Task<GiaoDienView> UpdateGiaoDienAsync(GiaoDienEdit model)
        {
            var giaoDien = await _context.GiaoDiens
                .FirstOrDefaultAsync(g => g.MaGiaoDien == model.MaGiaoDien);
            if (giaoDien == null)
                throw new Exception("Giao diện không tồn tại.");

            if (model.TenGiaoDien != null) giaoDien.TenGiaoDien = model.TenGiaoDien;
            if (model.Logo != null) giaoDien.Logo = model.Logo;
            if (model.Slider1 != null) giaoDien.Slider1 = model.Slider1;
            if (model.Slider2 != null) giaoDien.Slider2 = model.Slider2;
            if (model.Slider3 != null) giaoDien.Slider3 = model.Slider3;
            if (model.Slider4 != null) giaoDien.Slider4 = model.Slider4;
            if (model.Avt != null) giaoDien.Avt = model.Avt;
            if (model.TrangThai != null) giaoDien.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            return new GiaoDienView
            {
                MaGiaoDien = giaoDien.MaGiaoDien,
                TenGiaoDien = giaoDien.TenGiaoDien,
                Logo = giaoDien.Logo,
                Slider1 = giaoDien.Slider1,
                Slider2 = giaoDien.Slider2,
                Slider3 = giaoDien.Slider3,
                Slider4 = giaoDien.Slider4,
                Avt = giaoDien.Avt,
                NgayTao = giaoDien.NgayTao,
                TrangThai = giaoDien.TrangThai
            };
        }

        public async Task<bool> DeleteGiaoDienAsync(int maGiaoDien)
        {
            var giaoDien = await _context.GiaoDiens
                .FirstOrDefaultAsync(g => g.MaGiaoDien == maGiaoDien);
            if (giaoDien == null)
                return false;

            _context.GiaoDiens.Remove(giaoDien);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SetActiveGiaoDienAsync(int maGiaoDien)
        {
            var activeGiaoDiens = await _context.GiaoDiens
                .Where(g => g.TrangThai == 1 && g.MaGiaoDien != maGiaoDien)
                .ToListAsync();
            foreach (var gd in activeGiaoDiens)
            {
                gd.TrangThai = 0;
            }

            var selectedGiaoDien = await _context.GiaoDiens
                .FirstOrDefaultAsync(g => g.MaGiaoDien == maGiaoDien);
            if (selectedGiaoDien != null)
            {
                selectedGiaoDien.TrangThai = 1;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<GiaoDienView>> SearchGiaoDienAsync(string? tenGiaoDien, int? maGiaoDien, int? trangThai, DateTime? ngayTao)
        {
            var query = _context.GiaoDiens.AsQueryable();

            if (!string.IsNullOrEmpty(tenGiaoDien))
            {
                query = query.Where(g => g.TenGiaoDien.Contains(tenGiaoDien));
            }

            if (maGiaoDien.HasValue)
            {
                query = query.Where(g => g.MaGiaoDien == maGiaoDien.Value);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(g => g.TrangThai == trangThai.Value);
            }

            if (ngayTao.HasValue)
            {
                query = query.Where(g => g.NgayTao.HasValue && g.NgayTao.Value.Date == ngayTao.Value.Date);
            }

            var list = await query.ToListAsync();
            return list.Select(g => new GiaoDienView
            {
                MaGiaoDien = g.MaGiaoDien,
                TenGiaoDien = g.TenGiaoDien,
                Logo = g.Logo,
                Slider1 = g.Slider1,
                Slider2 = g.Slider2,
                Slider3 = g.Slider3,
                Slider4 = g.Slider4,
                Avt = g.Avt,
                NgayTao = g.NgayTao,
                TrangThai = g.TrangThai
            }).ToList();
        }
    }
}