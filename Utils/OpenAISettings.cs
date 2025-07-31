using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Utils
{
    public class OpenAISettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string DefaultModel { get; set; }
        public List<string> AvailableModels { get; set; }
    }

    public class AddToCartRequest
    {
        public string MaSanPham { get; set; }
        public int SoLuong { get; set; }
    }

    public class RequestOpenAIHinhAnh
    {
        [Required]
        public string CauHoi { get; set; }
        public List<byte[]>? HinhAnh { get; set; }
    }
}