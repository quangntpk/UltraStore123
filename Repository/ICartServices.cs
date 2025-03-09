using UltraStrore.Data.Temp;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface ICartServices
    {
        Task<APIResponse> ThemSanPham(ChiTietGioHangSanPhamCreate info);
        Task<GioHangView> GioHangViews(string MaKhachHang);
    }
}
