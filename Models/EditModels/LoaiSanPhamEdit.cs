using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.EditModels
{
    public class LoaiSanPhamEdit
    {
        [Required]
        public int MaLoaiSanPham { get; set; }

        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống")]
        [StringLength(40, ErrorMessage = "Tên loại sản phẩm tối đa 40 ký tự")]
        public string TenLoaiSanPham { get; set; }

        [StringLength(50, ErrorMessage = "Kí hiệu tối đa 50 ký tự")]
        public string? KiHieu { get; set; } // Nullable, không bắt buộc
    }
}
