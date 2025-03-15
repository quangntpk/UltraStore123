namespace UltraStrore.Models.CreateModels
{
    public class ChiTietGioHangComboCreate
    {
        public string? IDKhachHang { get; set; }
        public int? IDCombo { get; set; }
        public int? SoLuong { get; set; }
        public List<ChiTietChiTietCombo>? Detail {get;set;}
    }
}
