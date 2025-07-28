using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
namespace UltraStrore.Models.CreateModels
{
    public class SanPhamCreate
    {
        public string? TenSanPham { get; set; }
        public int? MaThuongHieu { get; set; }
        public int? LoaiSanPham { get; set; }
        public string? MoTa { get; set; }
        public string? MauSac { get; set; }
        public string? ChatLieu { get; set; }
        public int? GioiTinh { get; set; }
        public List<SanPhamEditDetail>? Details { get; set; }
        public List<byte[]>? HinhAnhs { get; set; }
    }
    public class FullCreateSanPham
    {
        public List<SanPhamCreate>? data { get; set; }
        public List<DetailHashTagSP>? ListHashTag { get; set; }
    }

}
