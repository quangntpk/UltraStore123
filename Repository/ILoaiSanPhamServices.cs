using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;

namespace UltraStrore.Repository
{
    public interface ILoaiSanPhamServices
    {
        Task<LoaiSanPhamView> CreateLoaiSanPhamAsync(LoaiSanPhamCreate model);
        Task<List<LoaiSanPhamView>> GetAllLoaiSanPhamAsync();
        Task<LoaiSanPhamView> GetLoaiSanPhamAsync(int maLoaiSanPham);
        Task<LoaiSanPhamView> UpdateLoaiSanPhamAsync(LoaiSanPhamEdit model);
        Task<bool> DeleteLoaiSanPhamAsync(int maLoaiSanPham);
        Task<List<SanPhamView>> GetSanPhamByLoaiAsync(int maLoaiSanPham);
        Task<List<LoaiSanPhamView>> SearchLoaiSanPhamAsync(string? tenLoai, string? kiHieu);
    }
}