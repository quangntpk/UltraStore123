namespace UltraStrore.Models.CreateModels
{
    public class ChiTietGioHangComboCreate
    {
        public string? IDKhachHang { get; set; }
        public int? IDCombo { get; set; }
        public int? SoLuong { get; set; }
        public List<ChiTietChiTietCombo>? Detail {get;set;}
        public int? ThanhTien { get; set; }
        public int? MaKhuyenMai { get; set; }
        public int? Percent { get; set; }
        public bool? DeadTime { get; set; }
    }
}
