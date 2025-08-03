using System.Threading.Tasks;
using UltraStrore.Utils;

namespace UltraStrore.Repository
{
    public interface IGoogleApisServices
    {
        Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request);
        Task<TextGenerationResponse> GenerateTextAsync(TextGenerationRequest request);
        Task<TextGenerationResponse> SearchProductsAsync(TextGenerationRequest request);
        Task<TextGenerationResponse> AddToCartAsync(TextGenerationRequest request);
    }
}