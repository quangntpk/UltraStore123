using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data.Temp
{
    public class SanPham
    {
        [Key]
        public string ID { get; set; }
        public string Name { get; set; }
        public int MaThuongHieu { get; set; }
        public int LoaiSanPham { get; set; } 

        public string? KichThuoc { get; set; }
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
        public DateOnly? NgayTao { get; set; }
        public int TrangThai { get; set; }
        public bool Example { get; set; }
        public string? MoTa { get; set; }
    }
}
