namespace UltraStrore.Models.ViewModels
{
    public class ChiTietGioHangSanPhamView
    {
        public int? ChiTietGioHangSanPham { get; set; }
        public string? IDSanPham { get; set; }
        public string? TenSanPham { get; set; }
        public string? MauSac {  get; set; }
        public string? KickThuoc { get; set; }
        public int? SoLuong { get; set; }
        public int? TienSanPham { get;set; }
        public byte[]? HinhAnh { get; set; }
    }
}
