using System.Text.Json;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using Microsoft.AspNetCore.SignalR;
using UltraStrore.Hubs;

namespace UltraStrore.Services
{
    public class GiaoDienServices : IGiaoDienServices
    {
        private readonly string _filePath;
        private readonly IHubContext<GiaoDienHub> _hubContext;

        public GiaoDienServices(IHubContext<GiaoDienHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "DanhMuc");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            _filePath = Path.Combine(directoryPath, "giaodien.json");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        private async Task<List<GiaoDien>> ReadGiaoDiensFromFileAsync()
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<GiaoDien>>(json) ?? new List<GiaoDien>();
        }

        private async Task WriteGiaoDiensToFileAsync(List<GiaoDien> giaoDiens)
        {
            var json = JsonSerializer.Serialize(giaoDiens, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<GiaoDienView> CreateGiaoDienAsync(GiaoDienCreate model)
        {
            var giaoDiens = await ReadGiaoDiensFromFileAsync();
            var newId = giaoDiens.Any() ? giaoDiens.Max(g => g.MaGiaoDien) + 1 : 1;

            var newGiaoDien = new GiaoDien
            {
                MaGiaoDien = newId,
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

            giaoDiens.Add(newGiaoDien);
            await WriteGiaoDiensToFileAsync(giaoDiens);

            var giaoDienView = new GiaoDienView
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

            await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienAdded", giaoDienView);

            return giaoDienView;
        }

        public async Task<List<GiaoDienView>> GetAllGiaoDienAsync()
        {
            var giaoDiens = await ReadGiaoDiensFromFileAsync();
            return giaoDiens.Select(g => new GiaoDienView
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
            var giaoDiens = await ReadGiaoDiensFromFileAsync();
            var giaoDien = giaoDiens.FirstOrDefault(g => g.MaGiaoDien == maGiaoDien);
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
            var giaoDiens = await ReadGiaoDiensFromFileAsync();
            var giaoDien = giaoDiens.FirstOrDefault(g => g.MaGiaoDien == model.MaGiaoDien);
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

            await WriteGiaoDiensToFileAsync(giaoDiens);

            var giaoDienView = new GiaoDienView
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

            await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienUpdated", giaoDienView);

            return giaoDienView;
        }

        public async Task<bool> DeleteGiaoDienAsync(int maGiaoDien)
        {
            var giaoDiens = await ReadGiaoDiensFromFileAsync();
            var giaoDien = giaoDiens.FirstOrDefault(g => g.MaGiaoDien == maGiaoDien);
            if (giaoDien == null)
                return false;

            giaoDiens.Remove(giaoDien);
            await WriteGiaoDiensToFileAsync(giaoDiens);

            await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienDeleted", maGiaoDien);

            return true;
        }

        public async Task SetActiveGiaoDienAsync(int maGiaoDien)
        {
            var giaoDiens = await ReadGiaoDiensFromFileAsync();
            var activeGiaoDiens = giaoDiens.Where(g => g.TrangThai == 1 && g.MaGiaoDien != maGiaoDien).ToList();
            foreach (var gd in activeGiaoDiens)
            {
                gd.TrangThai = 0;
            }

            var selectedGiaoDien = giaoDiens.FirstOrDefault(g => g.MaGiaoDien == maGiaoDien);
            if (selectedGiaoDien != null)
            {
                selectedGiaoDien.TrangThai = 1;
            }

            await WriteGiaoDiensToFileAsync(giaoDiens);

            await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienSetActive", maGiaoDien);
        }

        public async Task<List<GiaoDienView>> SearchGiaoDienAsync(string? tenGiaoDien, int? maGiaoDien, int? trangThai, DateTime? ngayTao)
        {
            var giaoDiens = await ReadGiaoDiensFromFileAsync();
            var query = giaoDiens.AsQueryable();

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

            var list = query.ToList();
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
