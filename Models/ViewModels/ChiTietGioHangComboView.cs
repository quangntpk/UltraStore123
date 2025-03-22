namespace UltraStrore.Models.ViewModels
{
    public class ChiTietGioHangComboView
    {
        public int? ChiTietGioHangCombo { get; set; }
        public int? IDCombo { get; set; }
        public string? TenCombo { get; set; }
        public List<SanPhamInGioHangCombo>? SanPhamList {get ; set; }
        public int SoLuong { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? Gia { get; set; }
    }
}
