using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Data
{
    public class ChiTietGioHangSupport
    {
        [Key]
        public int ID { get; set; }
        public string MaSanPham {  get; set; }
        public int ChiTietGioHang { get; set; }
        public int MaChiTietCombo { get; set; }
        public int SoLuong {  get; set; }
        public int Version { get; set; }
    }
}
