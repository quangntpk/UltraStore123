using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data
{
    public class HashTag
    {
        [Key]
        public int? MaHashTag { get; set; }
        public string? TenHashTag { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; }
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }
}