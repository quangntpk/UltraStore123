using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Utils
{
    public class GoogleApisSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }

    public class ImageGenerationRequest
    {
        [Required]
        public string TextPrompt { get; set; }
        [Required]
        public List<string> ImageBase64 { get; set; }
    }

    public class ImageGenerationResponse
    {
        public string GeneratedImageBase64 { get; set; }
        public string Message { get; set; }
    }

    public class TextGenerationRequest
    {
        [Required]
        public string TextPrompt { get; set; }
    }

    public class TextGenerationResponse
    {
        public string GeneratedText { get; set; }
        public string Message { get; set; }
    }
}