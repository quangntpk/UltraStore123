using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Models.EditModels
{
    public class SanPhamEdit
    {
        public string? ID { get; set; }
        public string? TenSanPham { get; set; }
        public int? MaThuongHieu { get; set; }
        public int? LoaiSanPham { get; set; }
        public string? MoTa { get; set; }
        public string? MauSac { get; set; }
        public string? ChatLieu { get; set; }
        public List<SanPhamEditDetail>? Details { get; set; }
        public List<byte[]>? HinhAnhs { get; set; }
    }
    public class FullInfoSanPhamEdit
    {
        public List<SanPhamEdit> data { get; set; }
        public List<DetailHashTagSP> hashtaglist { get; set; }
    }
}
