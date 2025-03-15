using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface ITinNhanServices
    {
        // Gửi tin nhắn mới
        Task<TinNhanView> SendMessageAsync(TinNhanCreate model);

        // Lấy cuộc trò chuyện giữa 2 người dùng (dựa theo ID của người gửi và người nhận)
        Task<List<TinNhanView>> GetConversationAsync(string nguoiGuiId, string nguoiNhanId);

        // Cập nhật nội dung tin nhắn (nếu cần)
        Task<TinNhanView> UpdateMessageAsync(TinNhanEdit model);

        // Xóa tin nhắn theo ID
        Task<bool> DeleteMessageAsync(int id);

        // Đánh dấu tin nhắn giữa 2 người là đã đọc
        Task MarkAsReadAsync(string nguoiGuiId, string nguoiNhanId);

        // Tìm kiếm tin nhắn liên quan đến 1 người dùng (người gửi hoặc người nhận)
        Task<List<TinNhanView>> SearchMessagesByUserIdAsync(string userId);

        // Lấy danh sách các ID người dùng đã từng nhắn tin
        Task<List<string>> GetDistinctUserIdsAsync();
    }
}
