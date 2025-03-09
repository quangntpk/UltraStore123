using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface IDanhSachDiaChiServices
    {
        Task<List<DanhSachDiaChiView>> GetDanhSachDiaChiList(string? searchTerm);
        Task<List<DanhSachDiaChiView>> GetDanhSachDiaChiByMaNguoiDung(string maNguoiDung);
        Task<DanhSachDiaChiView> GetDanhSachDiaChiById(int id);
        Task<DanhSachDiaChiView> CreateDanhSachDiaChi(DanhSachDiaChiCreate model);
        Task<DanhSachDiaChiView> UpdateDanhSachDiaChi(DanhSachDiaChiEdit model);
        Task<bool> DeleteDanhSachDiaChi(int id);
    }
}
