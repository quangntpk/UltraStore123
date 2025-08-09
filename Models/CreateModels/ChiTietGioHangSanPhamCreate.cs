namespace UltraStrore.Models.CreateModels
{
    public class ChiTietGioHangSanPhamCreate
    {
        public string? IDNguoiDung {  get; set; }
        public string? IDSanPham { get; set; }
        public string? MauSac { get; set; }
        public string? KichThuoc { get; set; }
        public int? SoLuong { get; set; }
        public int? MaKhuyenMai { get; set; }
        public DateOnly? KMDead { get; set; }
        public int? PercentDis { get; set; }
    }
}
