using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class ByteArrayConverter : JsonConverter<byte[]?>
    {
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            string base64 = reader.GetString();
            return string.IsNullOrEmpty(base64) ? null : Convert.FromBase64String(base64);
        }

        public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(Convert.ToBase64String(value));
        }
    }

    public class LoaiSanPhamServices : ILoaiSanPhamServices
    {
        private readonly string _jsonFilePath;

        public LoaiSanPhamServices()
        {
            _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc", "loaisanpham.json");

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

        private async Task<List<LoaiSanPham>> ReadJsonFileAsync()
        {
            try
            {
                using var stream = new FileStream(_jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var options = new JsonSerializerOptions
                {
                    Converters = { new ByteArrayConverter() }
                };
                return await JsonSerializer.DeserializeAsync<List<LoaiSanPham>>(stream, options) ?? new List<LoaiSanPham>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Lỗi khi đọc file JSON: {ex.Message}", ex);
            }
        }

        private async Task WriteJsonFileAsync(List<LoaiSanPham> loaiSanPhams)
        {
            try
            {
                using var stream = new FileStream(_jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new ByteArrayConverter() }
                };
                await JsonSerializer.SerializeAsync(stream, loaiSanPhams, options);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Lỗi khi ghi file JSON: {ex.Message}", ex);
            }
        }

        public async Task<LoaiSanPhamView> CreateLoaiSanPhamAsync(LoaiSanPhamCreate model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.TenLoaiSanPham))
                throw new ArgumentException("Tên loại sản phẩm không được để trống.", nameof(model.TenLoaiSanPham));

            var list = await ReadJsonFileAsync();
            int newId = list.Any() ? list.Max(l => l.MaLoaiSanPham) + 1 : 1;

            var newLoaiSanPham = new LoaiSanPham
            {
                MaLoaiSanPham = newId,
                TenLoaiSanPham = model.TenLoaiSanPham,
                KiHieu = model.KiHieu,
                KichThuoc = model.KichThuoc,
                HinhAnh = model.HinhAnh,
                TrangThai = model.TrangThai ?? 1
            };

            list.Add(newLoaiSanPham);
            await WriteJsonFileAsync(list);

            return new LoaiSanPhamView
            {
                MaLoaiSanPham = newLoaiSanPham.MaLoaiSanPham,
                TenLoaiSanPham = newLoaiSanPham.TenLoaiSanPham,
                KiHieu = newLoaiSanPham.KiHieu,
                KichThuoc = newLoaiSanPham.KichThuoc,
                HinhAnh = newLoaiSanPham.HinhAnh,
                TrangThai = newLoaiSanPham.TrangThai
            };
        }

        public async Task<List<LoaiSanPhamView>> GetAllLoaiSanPhamAsync(int? trangThai = null)
        {
            var list = await ReadJsonFileAsync();

            if (trangThai.HasValue)
            {
                list = list.Where(l => l.TrangThai == trangThai.Value).ToList();
            }

            return list.Select(l => new LoaiSanPhamView
            {
                MaLoaiSanPham = l.MaLoaiSanPham,
                TenLoaiSanPham = l.TenLoaiSanPham,
                KiHieu = l.KiHieu,
                KichThuoc = l.KichThuoc,
                HinhAnh = l.HinhAnh,
                TrangThai = l.TrangThai
            }).ToList();
        }

        public async Task<LoaiSanPhamView> GetLoaiSanPhamAsync(int maLoaiSanPham)
        {
            var list = await ReadJsonFileAsync();
            var loaiSanPham = list.FirstOrDefault(l => l.MaLoaiSanPham == maLoaiSanPham)
                ?? throw new KeyNotFoundException($"Loại sản phẩm với mã '{maLoaiSanPham}' không tồn tại.");

            return new LoaiSanPhamView
            {
                MaLoaiSanPham = loaiSanPham.MaLoaiSanPham,
                TenLoaiSanPham = loaiSanPham.TenLoaiSanPham,
                KiHieu = loaiSanPham.KiHieu,
                KichThuoc = loaiSanPham.KichThuoc,
                HinhAnh = loaiSanPham.HinhAnh,
                TrangThai = loaiSanPham.TrangThai
            };
        }

        public async Task<LoaiSanPhamView> UpdateLoaiSanPhamAsync(LoaiSanPhamEdit model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (model.MaLoaiSanPham <= 0)
                throw new ArgumentException("Mã loại sản phẩm không hợp lệ.", nameof(model.MaLoaiSanPham));

            if (string.IsNullOrWhiteSpace(model.TenLoaiSanPham))
                throw new ArgumentException("Tên loại sản phẩm không được để trống.", nameof(model.TenLoaiSanPham));

            var list = await ReadJsonFileAsync();
            var loaiSanPham = list.FirstOrDefault(l => l.MaLoaiSanPham == model.MaLoaiSanPham)
                ?? throw new KeyNotFoundException($"Loại sản phẩm với mã '{model.MaLoaiSanPham}' không tồn tại.");

            loaiSanPham.TenLoaiSanPham = model.TenLoaiSanPham;
            loaiSanPham.KiHieu = model.KiHieu;
            loaiSanPham.KichThuoc = model.KichThuoc;
            loaiSanPham.HinhAnh = model.HinhAnh;
            loaiSanPham.TrangThai = model.TrangThai ?? loaiSanPham.TrangThai;

            await WriteJsonFileAsync(list);

            return new LoaiSanPhamView
            {
                MaLoaiSanPham = loaiSanPham.MaLoaiSanPham,
                TenLoaiSanPham = loaiSanPham.TenLoaiSanPham,
                KiHieu = loaiSanPham.KiHieu,
                KichThuoc = loaiSanPham.KichThuoc,
                HinhAnh = loaiSanPham.HinhAnh,
                TrangThai = loaiSanPham.TrangThai
            };
        }

        public async Task<bool> DeleteLoaiSanPhamAsync(int maLoaiSanPham)
        {
            var list = await ReadJsonFileAsync();
            var loaiSanPham = list.FirstOrDefault(l => l.MaLoaiSanPham == maLoaiSanPham);
            if (loaiSanPham == null)
                return false;

            list.Remove(loaiSanPham);
            await WriteJsonFileAsync(list);
            return true;
        }

        public async Task<List<LoaiSanPhamView>> SearchLoaiSanPhamAsync(string? tenLoai, string? kiHieu, int? trangThai = null)
        {
            var list = await ReadJsonFileAsync();

            if (!string.IsNullOrWhiteSpace(tenLoai))
            {
                tenLoai = tenLoai.Trim().ToLower();
                list = list.Where(l => l.TenLoaiSanPham != null && l.TenLoaiSanPham.ToLower().Contains(tenLoai)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(kiHieu))
            {
                kiHieu = kiHieu.Trim().ToLower();
                list = list.Where(l => l.KiHieu != null && l.KiHieu.ToLower().Contains(kiHieu)).ToList();
            }

            if (trangThai.HasValue)
            {
                list = list.Where(l => l.TrangThai == trangThai.Value).ToList();
            }

            return list.Select(l => new LoaiSanPhamView
            {
                MaLoaiSanPham = l.MaLoaiSanPham,
                TenLoaiSanPham = l.TenLoaiSanPham,
                KiHieu = l.KiHieu,
                KichThuoc = l.KichThuoc,
                HinhAnh = l.HinhAnh,
                TrangThai = l.TrangThai
            }).ToList();
        }
    }
}