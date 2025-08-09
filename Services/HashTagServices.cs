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
    public class HashTagServices : IHashTagServices
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jsonFilePath;

        public HashTagServices(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "hashtag.json");

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

        private async Task<List<HashTag>> ReadJsonFileAsync()
        {
            using var stream = new FileStream(_jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<List<HashTag>>(stream) ?? new List<HashTag>();
        }

        private async Task WriteJsonFileAsync(List<HashTag> hashtags)
        {
            using var stream = new FileStream(_jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, hashtags, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<List<HashTagView>> GetAllHashTagAsync()
        {
            var list = await ReadJsonFileAsync();
            return list.Select(t => new HashTagView
            {
                MaHashTag = t.MaHashTag,
                TenHashTag = t.TenHashTag,
                TrangThai = t.TrangThai,
                HinhAnh = t.HinhAnh
            }).ToList();
        }

        public async Task<HashTagView> GetHashTagAsync(int maHashTag)
        {
            var list = await ReadJsonFileAsync();
            var hashTag = list.FirstOrDefault(t => t.MaHashTag == maHashTag)
                ?? throw new KeyNotFoundException("Hashtag không tồn tại.");

            return new HashTagView
            {
                MaHashTag = hashTag.MaHashTag,
                TenHashTag = hashTag.TenHashTag,
                TrangThai = hashTag.TrangThai,
                HinhAnh = hashTag.HinhAnh
            };
        }

        public async Task<HashTagView> CreateHashTagAsync(HashTagCreate model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.TenHashTag))
                throw new ArgumentException("Tên hashtag không được để trống.", nameof(model.TenHashTag));

            var list = await ReadJsonFileAsync();
            if (list.Any(t => t.TenHashTag == model.TenHashTag))
                throw new InvalidOperationException("Tên hashtag đã tồn tại.");

            var newHashTag = new HashTag
            {
                MaHashTag = list.Any() ? list.Max(t => t.MaHashTag) + 1 : 1,
                TenHashTag = model.TenHashTag,
                TrangThai = model.TrangThai,
                HinhAnh = model.HinhAnh
            };

            list.Add(newHashTag);
            await WriteJsonFileAsync(list);

            return new HashTagView
            {
                MaHashTag = newHashTag.MaHashTag,
                TenHashTag = newHashTag.TenHashTag,
                TrangThai = newHashTag.TrangThai,
                HinhAnh = newHashTag.HinhAnh
            };
        }

        public async Task<HashTagView> UpdateHashTagAsync(HashTagEdit model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var list = await ReadJsonFileAsync();
            var hashTag = list.FirstOrDefault(t => t.MaHashTag == model.MaHashTag)
                ?? throw new KeyNotFoundException("Hashtag không tồn tại.");

            if (!string.IsNullOrWhiteSpace(model.TenHashTag))
            {
                if (list.Any(t => t.TenHashTag == model.TenHashTag && t.MaHashTag != model.MaHashTag))
                    throw new InvalidOperationException("Tên hashtag đã tồn tại.");
            }

            hashTag.TenHashTag = model.TenHashTag ?? hashTag.TenHashTag;
            hashTag.TrangThai = model.TrangThai != default ? model.TrangThai : hashTag.TrangThai;
            hashTag.HinhAnh = model.HinhAnh ?? hashTag.HinhAnh;

            await WriteJsonFileAsync(list);

            return new HashTagView
            {
                MaHashTag = hashTag.MaHashTag,
                TenHashTag = hashTag.TenHashTag,
                TrangThai = hashTag.TrangThai,
                HinhAnh = hashTag.HinhAnh
            };
        }

        public async Task<bool> DeleteHashTagAsync(int maHashTag)
        {
            var list = await ReadJsonFileAsync();
            var hashTag = list.FirstOrDefault(t => t.MaHashTag == maHashTag);
            if (hashTag == null)
                return false;

            list.Remove(hashTag);
            await WriteJsonFileAsync(list);
            return true;
        }

        public async Task<List<HashTagView>> SearchHashTagAsync(string tenHashTag)
        {
            var list = await ReadJsonFileAsync();
            if (!string.IsNullOrWhiteSpace(tenHashTag))
            {
                tenHashTag = tenHashTag.Trim().ToLower();
                list = list.Where(t => t.TenHashTag.ToLower().Contains(tenHashTag)).ToList();
            }

            return list.Select(t => new HashTagView
            {
                MaHashTag = t.MaHashTag,
                TenHashTag = t.TenHashTag,
                TrangThai = t.TrangThai,
                HinhAnh = t.HinhAnh
            }).ToList();
        }

        public async Task<List<SanPhamView>> GetSanPhamByHashTagAsync(int maHashTag)
        {
            var list = await ReadJsonFileAsync();
            if (!list.Any(t => t.MaHashTag == maHashTag))
                throw new KeyNotFoundException("Hashtag không tồn tại.");

            var sanPhams = await _context.SanPhams
                .AsNoTracking()
                .Where(s => s.MaHashTag == maHashTag)
                .ToListAsync();

            return sanPhams.Select(s => new SanPhamView
            {
                ID = s.MaSanPham,
                Name = s.TenSanPham
            }).ToList();
        }
    }
}
