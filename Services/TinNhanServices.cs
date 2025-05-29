using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Hubs;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UltraStrore.Services
{
    public class TinNhanServices : ITinNhanServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hub;

        public TinNhanServices(ApplicationDbContext context, IHubContext<ChatHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task<TinNhanView> GuiTinNhanAsync(TinNhanCreate model)
        {
            string? tepDinhKemUrl = null;
            if (model.TepTin != null)
            {
                var fileName = $"{Guid.NewGuid()}_{model.TepTin.FileName}";
                var path = Path.Combine("wwwroot/uploads/chat", fileName);
                await using var stream = new FileStream(path, FileMode.Create);
                await model.TepTin.CopyToAsync(stream);
                tepDinhKemUrl = $"/uploads/chat/{fileName}";
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
                TepDinhKemUrl = tinNhan.TepDinhKemUrl,
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