using System.Threading.Tasks;
using UltraStrore.Helper;
using UltraStrore.Utils;

namespace UltraStrore.Repository
{
    public interface IOpenAIServices
    {
        Task<APIResponse> TraLoi(string userInput);
        Task<APIResponse> TraLoiLienHe(string userInput);
        Task<APIResponse> Response(RequestOpenAIHinhAnh? info);
        Task<APIResponse> PhanLoaiGopY(string noiDung);
        Task<APIResponse> TraLoiUpgrade(string userInput);
        Task<APIResponse> ThemVaoGioHang(AddToCartRequest request);
    }
}