using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UltraStrore.Data;
using UltraStrore.Hubs;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class TinNhanServices : ITinNhanServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hub;
        private readonly IWebHostEnvironment _environment;

        public TinNhanServices(ApplicationDbContext context, IHubContext<ChatHub> hub, IWebHostEnvironment environment)
        {
            _context = context;
            _hub = hub;
            _environment = environment;
        }

        public async Task<TinNhanView> GuiTinNhanAsync(TinNhanCreate model)
        {
            string? tepDinhKemUrl = null;
            if (model.TepTin != null)
            {
                try
                {
                    var fileName = $"{Guid.NewGuid()}_{model.TepTin.FileName}";
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "chat");
                    var path = Path.Combine(uploadsFolder, fileName);

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    await using var stream = new FileStream(path, FileMode.Create);
                    await model.TepTin.CopyToAsync(stream);

                    tepDinhKemUrl = $"/uploads/chat/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lưu tệp: {ex.Message}");
                    throw;
                }
            }

            var tinNhan = new TinNhan
            {
                NguoiGuiId = model.NguoiGuiId,
                NguoiNhanId = model.NguoiNhanId,
                NoiDung = model.NoiDung,
                KieuTinNhan = model.KieuTinNhan ?? "text",
                TepDinhKemUrl = tepDinhKemUrl,
                NgayTao = DateTime.Now,
                TrangThai = "sent"
            };

            _context.TinNhans.Add(tinNhan);
            await _context.SaveChangesAsync();

            var result = new TinNhanView
            {
                MaTinNhan = tinNhan.MaTinNhan ?? 0,
                NguoiGuiId = tinNhan.NguoiGuiId,
                NguoiNhanId = tinNhan.NguoiNhanId,
                NoiDung = tinNhan.NoiDung,
                KieuTinNhan = tinNhan.KieuTinNhan,
                TepDinhKemUrl = tepDinhKemUrl,
                NgayTao = tinNhan.NgayTao ?? DateTime.MinValue,
                TrangThai = tinNhan.TrangThai
            };

            await _hub.Clients.User(tinNhan.NguoiNhanId).SendAsync("NhanTinNhan", result);
            return result;
        }

        public async Task<IEnumerable<TinNhanView>> LayTinNhanGiuaHaiNguoiAsync(string nguoiGuiId, string nguoiNhanId)
        {
            var tinNhans = await _context.TinNhans
                .Where(t => (t.NguoiGuiId == nguoiGuiId && t.NguoiNhanId == nguoiNhanId)
                         || (t.NguoiGuiId == nguoiNhanId && t.NguoiNhanId == nguoiGuiId))
                .OrderBy(t => t.NgayTao)
                .Select(t => new TinNhanView
                {
                    MaTinNhan = t.MaTinNhan ?? 0,
                    NguoiGuiId = t.NguoiGuiId,
                    NguoiNhanId = t.NguoiNhanId,
                    NoiDung = t.NoiDung,
                    KieuTinNhan = t.KieuTinNhan,
                    TepDinhKemUrl = t.TepDinhKemUrl,
                    NgayTao = t.NgayTao ?? DateTime.MinValue,
                    TrangThai = t.TrangThai
                })
                .ToListAsync();

            return tinNhans;
        }

        public async Task<IEnumerable<TinNhanView>> LayDanhSachThreadsAsync(string userId)
        {
            var query = _context.TinNhans
                .Where(t => t.NguoiGuiId == userId || t.NguoiNhanId == userId)
                .OrderByDescending(t => t.NgayTao)
                .AsEnumerable()
                .GroupBy(t => t.NguoiGuiId == userId ? t.NguoiNhanId : t.NguoiGuiId)
                .Select(g => g.First())
                .Select(t => new TinNhanView
                {
                    MaTinNhan = t.MaTinNhan ?? 0,
                    NguoiGuiId = t.NguoiGuiId,
                    NguoiNhanId = t.NguoiNhanId,
                    NoiDung = t.NoiDung,
                    KieuTinNhan = t.KieuTinNhan,
                    TepDinhKemUrl = t.TepDinhKemUrl,
                    NgayTao = t.NgayTao ?? DateTime.MinValue,
                    TrangThai = t.TrangThai
                });

            return await Task.FromResult(query);
        }
    }
}