namespace UltraStrore.Helper
{
    public class OrderItemResponse
    {
        public int MaChiTietDh { get; set; }
        public string TenSanPham { get; set; } = null!;
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        public string? HinhAnh { get; set; }
        public bool LaCombo { get; set; }
    }
}
