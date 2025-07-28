namespace UltraStrore.Models.ViewModels
{
    public class SanPhamView
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string? ThuongHieu { get; set; }
        public string? LoaiSanPham { get; set; }
        public List<string>? KichThuoc { get; set; }
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
        public string? MoTa { get; set; }
        public string? ChatLieu { get; set; }
        public List<string>? MauSac { get; set; }
        public List<byte[]>? Hinh { get; set; }
        public DateOnly? NgayTao { get; set; }
        public int? TrangThai { get; set; }
        public int? SoLuongDaBan { get; set; }
        public string? GioiTinh { get; set; }
        public bool? Hot { get; set; }
        public List<DetailHashTagSP>? ListHashTag { get; set; }
    }
}
