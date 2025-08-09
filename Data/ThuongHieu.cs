using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data
{
    public class ThuongHieu
    {
        [Key]
        public int MaThuongHieu { get; set; }
        public string? TenThuongHieu { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; } = 1;
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}
