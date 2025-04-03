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

namespace UltraStrore.Services
{
    public class ThuongHieuServices : IThuongHieuServices
    {
        private readonly ApplicationDbContext _context;

        public ThuongHieuServices(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ThuongHieuView>> GetAllThuongHieuAsync()
        {
            var list = await _context.ThuongHieus
                .AsNoTracking()
                .ToListAsync();

            return list.Select(t => new ThuongHieuView
            {
                MaThuongHieu = t.MaThuongHieu,
                TenThuongHieu = t.TenThuongHieu,
                HinhAnh = t.HinhAnh
            }).ToList();
        }

        public async Task<ThuongHieuView> GetThuongHieuAsync(int maThuongHieu)
        {
            var thuongHieu = await _context.ThuongHieus
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaThuongHieu == maThuongHieu)
                ?? throw new KeyNotFoundException("Thương hiệu không tồn tại.");

            return new ThuongHieuView
            {
                MaThuongHieu = thuongHieu.MaThuongHieu,
                TenThuongHieu = thuongHieu.TenThuongHieu,
                HinhAnh = thuongHieu.HinhAnh
            };
        }

        public async Task<ThuongHieuView> CreateThuongHieuAsync(ThuongHieuCreate model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.TenThuongHieu))
                throw new ArgumentException("Tên thương hiệu không được để trống.", nameof(model.TenThuongHieu));

            var newThuongHieu = new ThuongHieu
            {
                TenThuongHieu = model.TenThuongHieu,
                HinhAnh = model.HinhAnh
            };

            _context.ThuongHieus.Add(newThuongHieu);
            await _context.SaveChangesAsync();

            return new ThuongHieuView
            {
                MaThuongHieu = newThuongHieu.MaThuongHieu,
                TenThuongHieu = newThuongHieu.TenThuongHieu,
                HinhAnh = newThuongHieu.HinhAnh
            };
        }

        public async Task<ThuongHieuView> UpdateThuongHieuAsync(ThuongHieuEdit model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var thuongHieu = await _context.ThuongHieus
                .FirstOrDefaultAsync(t => t.MaThuongHieu == model.MaThuongHieu)
                ?? throw new KeyNotFoundException("Thương hiệu không tồn tại.");

            thuongHieu.TenThuongHieu = model.TenThuongHieu ?? thuongHieu.TenThuongHieu;
            thuongHieu.HinhAnh = model.HinhAnh ?? thuongHieu.HinhAnh;

            await _context.SaveChangesAsync();

            return new ThuongHieuView
            {
                MaThuongHieu = thuongHieu.MaThuongHieu,
                TenThuongHieu = thuongHieu.TenThuongHieu,
                HinhAnh = thuongHieu.HinhAnh
            };
        }

        public async Task<bool> DeleteThuongHieuAsync(int maThuongHieu)
        {
            var thuongHieu = await _context.ThuongHieus
                .FirstOrDefaultAsync(t => t.MaThuongHieu == maThuongHieu);

            if (thuongHieu == null)
                return false;

            _context.ThuongHieus.Remove(thuongHieu);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ThuongHieuView>> SearchThuongHieuAsync(string tenThuongHieu)
        {
            var query = _context.ThuongHieus.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tenThuongHieu))
            {
                query = query.Where(t => t.TenThuongHieu.Contains(tenThuongHieu, StringComparison.OrdinalIgnoreCase));
            }

            var list = await query.ToListAsync();
            return list.Select(t => new ThuongHieuView
            {
                MaThuongHieu = t.MaThuongHieu,
                TenThuongHieu = t.TenThuongHieu,
                HinhAnh = t.HinhAnh
            }).ToList();
        }

        public async Task<List<SanPhamView>> GetSanPhamByThuongHieuAsync(int maThuongHieu)
        {
            var thuongHieuExists = await _context.ThuongHieus.AnyAsync(t => t.MaThuongHieu == maThuongHieu);
            if (!thuongHieuExists)
                throw new KeyNotFoundException("Thương hiệu không tồn tại.");

            var sanPhams = await _context.SanPhams
                .AsNoTracking()
                .Where(s => s.MaThuongHieu == maThuongHieu)
                .ToListAsync();

            return sanPhams.Select(s => new SanPhamView
            {
                ID = s.MaSanPham,
                Name = s.TenSanPham
            }).ToList();
        }
    }
}