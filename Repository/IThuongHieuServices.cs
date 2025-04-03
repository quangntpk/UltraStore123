using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UltraStrore.Repository
{
    public interface IThuongHieuServices
    {
        Task<List<ThuongHieuView>> GetAllThuongHieuAsync();
        Task<ThuongHieuView> GetThuongHieuAsync(int maThuongHieu);
        Task<ThuongHieuView> CreateThuongHieuAsync(ThuongHieuCreate model);
        Task<ThuongHieuView> UpdateThuongHieuAsync(ThuongHieuEdit model);
        Task<bool> DeleteThuongHieuAsync(int maThuongHieu);
        Task<List<SanPhamView>> GetSanPhamByThuongHieuAsync(int maThuongHieu);
        Task<List<ThuongHieuView>> SearchThuongHieuAsync(string tenThuongHieu);
    }
}