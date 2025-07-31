using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Utils
{
    public class OpenAISettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string DefaultModel { get; set; }
    }
}