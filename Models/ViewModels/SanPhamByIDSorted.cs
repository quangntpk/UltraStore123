using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;

namespace UltraStrore.Models.ViewModels
{
    public class SanPhamByIDSorted
    {
        public string? ID { get; set; }
        public string? TenSanPham { get; set; }
        public string? MaThuongHieu { get; set; }
        public string? LoaiSanPham { get; set; }
        public int? TH { get; set; }
        public int? LSP { get; set; }
        public string? MauSac {  get; set; }
        public string? MoTa { get; set; }
        public string? ChatLieu {  get; set; }
        public int? GioiTinh { get; set; }
        public List<SanPhamEditDetail>? Details { get; set; }
        public List<byte[]>? HinhAnhs { get; set; }
        public MoTaSanPhamCreateModel? MoTaChiTiet { get; set; }
    }
}
