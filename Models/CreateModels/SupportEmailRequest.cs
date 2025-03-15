using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.CreateModels
{
    public class SupportEmailRequest
    {
        [Required(ErrorMessage = "Email người nhận là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string ToEmail { get; set; }

        [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc.")]

        public string Message { get; set; }
    }
}