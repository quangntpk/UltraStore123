using UltraStrore.Data;

namespace UltraStrore.Models.ViewModels
{
    public class BinhLuanView
    {
        public int? MaBinhLuan { get; set; }
        public string? MaSanPham { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? NoiDungBinhLuan { get; set; }
        public int? SoTimBinhLuan { get; set; }
        public double? DanhGia { get; set; }
        public int? TrangThai { get; set; }
        public DateTime? NgayBinhLuan { get; set; }

    }
}
