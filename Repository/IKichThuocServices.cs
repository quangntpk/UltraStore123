using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltraStrore.Helper;

namespace UltraStrore.Repository
{
    public interface IKichThuocServices
    {
        Task<List<KichThuocView>> GetAllKichThuocAsync();
        Task<KichThuocView> GetKichThuocAsync(int maKichThuoc);
        Task<APIResponse> CreateKichThuocAsync(KichThuocCreate model);
        Task<KichThuocView> UpdateKichThuocAsync(KichThuocEdit model);
        Task<bool> DeleteKichThuocAsync(int maKichThuoc);
        Task<List<KichThuocView>> SearchKichThuocAsync(string tenKichThuoc);
    }
}