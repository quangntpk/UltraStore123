namespace UltraStrore.Models.ViewModels
{
    public class ChiTietKhuyenMaiView
    {
        public int Id { get; set; }
        public string? IdSanPham { get; set; }
        public int? IdCombo { get; set; }
        public string? TenSanPhamCombo { get; set; }
        public float? GiaMoi { get; set; }
        public int? Percent { get; set; }
        public int? GiaGoc { get; set; }
        public List<byte[]>? HinhAnh { get; set; }
        
    }
}
