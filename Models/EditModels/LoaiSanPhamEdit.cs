using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.EditModels
{
    public class LoaiSanPhamEdit
    {
        public int MaLoaiSanPham { get; set; }
        public string? TenLoaiSanPham { get; set; }
        public string? KiHieu { get; set; }
        public List<string>? KichThuoc { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; }
    }
}
