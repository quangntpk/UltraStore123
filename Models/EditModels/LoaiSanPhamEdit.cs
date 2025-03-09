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
    }
}
