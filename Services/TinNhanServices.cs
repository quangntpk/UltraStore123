using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Services
{
    public class TinNhanServices : ITinNhanServices
    {
        private readonly ApplicationDbContext _context;

        public TinNhanServices(ApplicationDbContext context)
        {
            _context = context;
        }

        // Gửi tin nhắn mới
        public async Task<TinNhanView> SendMessageAsync(TinNhanCreate model)
        {
            var message = new TinNhan
            {
                NguoiGuiId = model.NguoiGuiId,
                NguoiNhanId = model.NguoiNhanId,
                NoiDung = model.NoiDung,
                NgayTao = model.NgayTao != default(DateTime) ? model.NgayTao : DateTime.Now,
                TrangThai = model.TrangThai
            };

            _context.TinNhans.Add(message);
            await _context.SaveChangesAsync();

            return new TinNhanView
            {
                MaTinNhan = message.MaTinNhan,
                NguoiGuiId = message.NguoiGuiId,
                NguoiNhanId = message.NguoiNhanId,
                NoiDung = message.NoiDung,
                NgayTao = message.NgayTao,
                TrangThai = message.TrangThai
            };
        }

        // Lấy cuộc trò chuyện giữa 2 người dùng
        public async Task<List<TinNhanView>> GetConversationAsync(string nguoiGuiId, string nguoiNhanId)
        {
            var conversation = await _context.TinNhans
                .Where(t => (t.NguoiGuiId == nguoiGuiId && t.NguoiNhanId == nguoiNhanId)
                         || (t.NguoiGuiId == nguoiNhanId && t.NguoiNhanId == nguoiGuiId))
                .OrderBy(t => t.NgayTao)
                .ToListAsync();

            return conversation.Select(t => new TinNhanView
            {
                MaTinNhan = t.MaTinNhan,
                NguoiGuiId = t.NguoiGuiId,
                NguoiNhanId = t.NguoiNhanId,
                NoiDung = t.NoiDung,
                NgayTao = t.NgayTao,
                TrangThai = t.TrangThai
            }).ToList();
        }

        // Cập nhật nội dung tin nhắn
        public async Task<TinNhanView> UpdateMessageAsync(TinNhanEdit model)
        {
            var message = await _context.TinNhans.FirstOrDefaultAsync(t => t.MaTinNhan == model.MaTinNhan);
            if (message == null)
                throw new Exception("Tin nhắn không tồn tại.");

            message.NoiDung = model.NoiDung;
            await _context.SaveChangesAsync();

            return new TinNhanView
            {
                MaTinNhan = message.MaTinNhan,
                NguoiGuiId = message.NguoiGuiId,
                NguoiNhanId = message.NguoiNhanId,
                NoiDung = message.NoiDung,
                NgayTao = message.NgayTao,
                TrangThai = message.TrangThai
            };
        }

        // Xóa tin nhắn theo ID
        public async Task<bool> DeleteMessageAsync(int id)
        {
            var message = await _context.TinNhans.FirstOrDefaultAsync(t => t.MaTinNhan == id);
            if (message == null)
                return false;

            _context.TinNhans.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        // Đánh dấu tin nhắn giữa 2 người là đã đọc
        public async Task MarkAsReadAsync(string nguoiGuiId, string nguoiNhanId)
        {
            var messages = await _context.TinNhans
                .Where(t => t.NguoiNhanId == nguoiNhanId &&
                            t.NguoiGuiId == nguoiGuiId &&
                            t.TrangThai == "Unread")
                .ToListAsync();

            if (messages.Any())
            {
                messages.ForEach(t => t.TrangThai = "Read");
                await _context.SaveChangesAsync();
            }
        }

        // Tìm kiếm tin nhắn liên quan đến 1 người dùng (người gửi hoặc người nhận)
        public async Task<List<TinNhanView>> SearchMessagesByUserIdAsync(string userId)
        {
            var messages = await _context.TinNhans
                .Where(t => t.NguoiGuiId == userId || t.NguoiNhanId == userId)
                .OrderBy(t => t.NgayTao)
                .ToListAsync();

            return messages.Select(t => new TinNhanView
            {
                MaTinNhan = t.MaTinNhan,
                NguoiGuiId = t.NguoiGuiId,
                NguoiNhanId = t.NguoiNhanId,
                NoiDung = t.NoiDung,
                NgayTao = t.NgayTao,
                TrangThai = t.TrangThai
            }).ToList();
        }

        // Lấy danh sách các ID người dùng đã từng nhắn tin (kết hợp sender và receiver)
        public async Task<List<string>> GetDistinctUserIdsAsync()
        {
            var senderIds = await _context.TinNhans
                                .Select(t => t.NguoiGuiId)
                                .Distinct()
                                .ToListAsync();
            var receiverIds = await _context.TinNhans
                                .Select(t => t.NguoiNhanId)
                                .Distinct()
                                .ToListAsync();
            return senderIds.Union(receiverIds).ToList();
        }
    }
}
