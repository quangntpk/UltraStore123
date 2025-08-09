using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface IKhuyenMaiServices
    {
        Task<List<KhuyenMaiView>> ListKhuyenMaiUser(int? id);
        Task<List<KhuyenMaiView>> ListKhuyenMaiAdmin(int? id);
        Task<APIResponse> MoTaKhuyenMaiCreate(MoTaKhuyenMai moTaKhuyenMai);
        Task<APIResponse> KhuyenMaiCreate(KhuyenMaiCreate data);
        Task<APIResponse> KhuyenMaiUpdate(KhuyenMaiEdit data);
        Task<APIResponse> MoTaKhuyenMaiEdit(MoTaKhuyenMai data);

    }
}
