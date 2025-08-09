using System.Threading.Tasks;

namespace UltraStrore.Repository
{
    public interface IOpenAIServices
    {
        Task<string> GetChatResponseAsync(string userQuery);
    }
}