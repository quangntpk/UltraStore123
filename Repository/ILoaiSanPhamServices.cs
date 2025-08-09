using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltraStrore.Repository
{
    public interface ILoaiSanPhamServices
    {
        Task<LoaiSanPhamView> CreateLoaiSanPhamAsync(LoaiSanPhamCreate model);
        Task<List<LoaiSanPhamView>> GetAllLoaiSanPhamAsync(int? trangThai = null);
        Task<LoaiSanPhamView> GetLoaiSanPhamAsync(int maLoaiSanPham);
        Task<LoaiSanPhamView> UpdateLoaiSanPhamAsync(LoaiSanPhamEdit model);
        Task<bool> DeleteLoaiSanPhamAsync(int maLoaiSanPham);
        Task<List<LoaiSanPhamView>> SearchLoaiSanPhamAsync(string? tenLoai, string? kiHieu, int? trangThai = null);
    }

}