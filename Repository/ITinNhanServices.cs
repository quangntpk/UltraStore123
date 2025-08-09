using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltraStrore.Repository
{
    public interface ITinNhanServices
    {
        Task<TinNhanView> GuiTinNhanAsync(TinNhanCreate model);
        Task<IEnumerable<TinNhanView>> LayTinNhanGiuaHaiNguoiAsync(string nguoiGuiId, string nguoiNhanId);
        Task<IEnumerable<TinNhanView>> LayDanhSachThreadsAsync(string userId);
    }
}