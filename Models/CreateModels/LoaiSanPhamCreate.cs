using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.CreateModels
{
    public class LoaiSanPhamCreate
    {
        public int MaLoaiSanPham { get; set; }
        public string TenLoaiSanPham { get; set; }
        public string? KiHieu { get; set; }
        public byte[]? HinhAnh { get; set; }
    }
}
