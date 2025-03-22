using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface IGiaoDienServices
    {
        Task<GiaoDienView> CreateGiaoDienAsync(GiaoDienCreate model);
        Task<List<GiaoDienView>> GetAllGiaoDienAsync();
        Task<GiaoDienView> GetGiaoDienAsync(int maGiaoDien);
        Task<GiaoDienView> UpdateGiaoDienAsync(GiaoDienEdit model);
        Task<bool> DeleteGiaoDienAsync(int maGiaoDien);
    }
}