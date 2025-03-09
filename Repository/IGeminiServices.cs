using UltraStrore.Helper;

namespace UltraStrore.Repository
{
    public interface IGeminiServices
    {
        Task<APIResponse> TraLoi(string userInput);
    }
}
