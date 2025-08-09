using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.EditModels
{
    public class ThuongHieuEdit
    {
        [Required]
        public int MaThuongHieu { get; set; }
        public string TenThuongHieu { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; }
    }
}