namespace UltraStrore.Models.EditModels
{
    public class TinNhanEdit
    {
        public int? MaTinNhan { get; set; }               // ID tin nhắn cần sửa
        public string? NoiDung { get; set; }             // Chỉ cho phép sửa nội dung
        public string? TrangThai { get; set; }           // Cập nhật trạng thái: "seen", "deleted", v.v.
    }
}
