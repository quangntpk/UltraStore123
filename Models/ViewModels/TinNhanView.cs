namespace UltraStrore.Models.ViewModels
{
    public class TinNhanView
    {
        public int MaTinNhan { get; set; }
        public string NguoiGuiId { get; set; }
        public string NguoiNhanId { get; set; }
        public string? NoiDung { get; set; }
        public string? KieuTinNhan { get; set; }         // "text", "image", "file", "emoji"
        public string? TepDinhKemUrl { get; set; }       // URL file nếu có
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; }

        // Gợi ý mở rộng: hiện thêm tên/người gửi nếu cần
        public string? TenNguoiGui { get; set; }
        public string? TenNguoiNhan { get; set; }
    }
}
