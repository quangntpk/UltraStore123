using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;

namespace UltraStrore.Repository
{
    public interface IGeminiServices
    {
        Task<APIResponse> TraLoi(string userInput);
        Task<APIResponse> Response(RequestGeminiHinhAnh? info);
    }
}
