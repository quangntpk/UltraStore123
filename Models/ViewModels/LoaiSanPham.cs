using UltraStrore.Data;

namespace UltraStrore.Models.ViewModels
{
    public class LoaiSanPham
    {
        public int MaLoaiSanPham { get; set; }
        public string? TenLoaiSanPham { get; set; }
        public string? KiHieu { get; set; }
        public List<string>? KichThuoc { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; }
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
