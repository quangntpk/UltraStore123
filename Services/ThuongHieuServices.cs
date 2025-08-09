using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class ThuongHieuServices : IThuongHieuServices
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jsonFilePath;

        public ThuongHieuServices(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "thuonghieu.json");

            var directory = Path.GetDirectoryName(_jsonFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");
            }
        }

        private async Task<List<ThuongHieu>> ReadJsonFileAsync()
        {
            using var stream = new FileStream(_jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<List<ThuongHieu>>(stream) ?? new List<ThuongHieu>();
        }

        private async Task WriteJsonFileAsync(List<ThuongHieu> thuongHieus)
        {
            using var stream = new FileStream(_jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, thuongHieus, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<List<ThuongHieuView>> GetAllThuongHieuAsync()
        {
            var list = await ReadJsonFileAsync();
            return list.Select(t => new ThuongHieuView
            {
                MaThuongHieu = t.MaThuongHieu,
                TenThuongHieu = t.TenThuongHieu,
                TrangThai = t.TrangThai,
                HinhAnh = t.HinhAnh
            }).ToList();
        }

        public async Task<ThuongHieuView> GetThuongHieuAsync(int maThuongHieu)
        {
            var list = await ReadJsonFileAsync();
            var thuongHieu = list.FirstOrDefault(t => t.MaThuongHieu == maThuongHieu)
                ?? throw new KeyNotFoundException("Thương hiệu không tồn tại.");

            return new ThuongHieuView
            {
                MaThuongHieu = thuongHieu.MaThuongHieu,
                TenThuongHieu = thuongHieu.TenThuongHieu,
                TrangThai = thuongHieu.TrangThai,
                HinhAnh = thuongHieu.HinhAnh
            };
        }

        public async Task<ThuongHieuView> CreateThuongHieuAsync(ThuongHieuCreate model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.TenThuongHieu))
                throw new ArgumentException("Tên thương hiệu không được để trống.", nameof(model.TenThuongHieu));

            var list = await ReadJsonFileAsync();
            if (list.Any(t => t.TenThuongHieu == model.TenThuongHieu))
                throw new InvalidOperationException("Tên thương hiệu đã tồn tại.");

            var newThuongHieu = new ThuongHieu
            {
                MaThuongHieu = list.Any() ? list.Max(t => t.MaThuongHieu) + 1 : 1,
                TenThuongHieu = model.TenThuongHieu,
                TrangThai = model.TrangThai,
                HinhAnh = model.HinhAnh
            };

            list.Add(newThuongHieu);
            await WriteJsonFileAsync(list);

            return new ThuongHieuView
            {
                MaThuongHieu = newThuongHieu.MaThuongHieu,
                TenThuongHieu = newThuongHieu.TenThuongHieu,
                TrangThai = newThuongHieu.TrangThai,
                HinhAnh = newThuongHieu.HinhAnh
            };
        }

        public async Task<ThuongHieuView> UpdateThuongHieuAsync(ThuongHieuEdit model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var list = await ReadJsonFileAsync();
            var thuongHieu = list.FirstOrDefault(t => t.MaThuongHieu == model.MaThuongHieu)
                ?? throw new KeyNotFoundException("Thương hiệu không tồn tại.");

            if (!string.IsNullOrWhiteSpace(model.TenThuongHieu))
            {
                if (list.Any(t => t.TenThuongHieu == model.TenThuongHieu && t.MaThuongHieu != model.MaThuongHieu))
                    throw new InvalidOperationException("Tên thương hiệu đã tồn tại.");
            }

            thuongHieu.TenThuongHieu = model.TenThuongHieu ?? thuongHieu.TenThuongHieu;
            thuongHieu.TrangThai = model.TrangThai ?? thuongHieu.TrangThai;
            thuongHieu.HinhAnh = model.HinhAnh ?? thuongHieu.HinhAnh;

            await WriteJsonFileAsync(list);

            return new ThuongHieuView
            {
                MaThuongHieu = thuongHieu.MaThuongHieu,
                TenThuongHieu = thuongHieu.TenThuongHieu,
                TrangThai = thuongHieu.TrangThai,
                HinhAnh = thuongHieu.HinhAnh
            };
        }

        public async Task<bool> DeleteThuongHieuAsync(int maThuongHieu)
        {
            var list = await ReadJsonFileAsync();
            var thuongHieu = list.FirstOrDefault(t => t.MaThuongHieu == maThuongHieu);
            if (thuongHieu == null)
                return false;

            var hasSanPhams = await _context.SanPhams
                .AnyAsync(s => s.MaThuongHieu == maThuongHieu);

            list.Remove(thuongHieu);
            await WriteJsonFileAsync(list);
            return true;
        }

        public async Task<List<ThuongHieuView>> SearchThuongHieuAsync(string tenThuongHieu)
        {
            var list = await ReadJsonFileAsync();
            if (!string.IsNullOrWhiteSpace(tenThuongHieu))
            {
                tenThuongHieu = tenThuongHieu.Trim().ToLower();
                list = list.Where(t => t.TenThuongHieu.ToLower().Contains(tenThuongHieu)).ToList();
            }

            return list.Select(t => new ThuongHieuView
            {
                MaThuongHieu = t.MaThuongHieu,
                TenThuongHieu = t.TenThuongHieu,
                TrangThai = t.TrangThai,
                HinhAnh = t.HinhAnh
            }).ToList();
        }

        public async Task<List<SanPhamView>> GetSanPhamByThuongHieuAsync(int maThuongHieu)
        {
            var list = await ReadJsonFileAsync();
            if (!list.Any(t => t.MaThuongHieu == maThuongHieu))
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
