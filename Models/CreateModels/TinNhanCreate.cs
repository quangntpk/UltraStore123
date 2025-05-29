namespace UltraStrore.Models.CreateModels
{
    public class TinNhanCreate
    {
        public string? NguoiGuiId { get; set; }           // Mã người gửi
        public string? NguoiNhanId { get; set; }          // Mã người nhận
        public string? NoiDung { get; set; }             // Nội dung văn bản hoặc emoji
        public string? KieuTinNhan { get; set; }         // "text", "image", "file", "emoji"
        public IFormFile? TepTin { get; set; }           // Ảnh hoặc file đính kèm
    }
}
