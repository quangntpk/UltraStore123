using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data.Temp
{
    public class LienHe
    {
        [Key]
        public int MaLienHe { get; set; }
        public string? HoTen { get; set; }
        public string? Sdt { get; set; }
        public string? NoiDung { get; set; }
        public string? Email { get; set; }
        public int? TrangThai { get; set; }

    }
}
