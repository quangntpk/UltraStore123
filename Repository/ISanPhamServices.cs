using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Models.CreateModels;
namespace UltraStrore.Repository
{
    public interface ISanPhamServices
    {
        Task<List<SanPhamView>> ListSanPham(string? id);
        Task<List<SanPham>> SanPhamByID(string? id);
        Task<List<SanPhamByIDSorted>> SanPhamByIDSorteds(string? id);
        Task<APIResponse> EditSanPham(List<SanPhamEdit> data);
        Task<APIResponse> CreateSanPham(List<SanPhamCreate> data);
        Task<APIResponse> DeleteSanPham(string? id);
        Task<APIResponse> ActiveSanPham(string? id);
    }
}
