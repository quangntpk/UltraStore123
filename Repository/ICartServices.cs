using UltraStrore.Data.Temp;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface ICartServices
    {
        Task<APIResponse> ThemSanPham(ChiTietGioHangSanPhamCreate info);
        Task<GioHangView> GioHangViews(string MaKhachHang);
        Task<APIResponse> ThemCombo(ChiTietGioHangComboCreate info);
        Task<APIResponse> GiamSoLuongSanPham(TangGiamSoLuongGioHang info);
        Task<APIResponse> TangSoLuongSanPham(TangGiamSoLuongGioHang info);
        Task<APIResponse> XoaChiTietGioHang(TangGiamSoLuongGioHang info);
        Task<APIResponse> XoaChiTietGiohangCombo(TangGiamSoLuongGioHang info);
        Task<APIResponse> XoaVersionComboGioHang(GioHangComboVersion info);
    }
}
