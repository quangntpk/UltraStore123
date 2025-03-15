using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.CreateModels
{
    public class LoaiSanPhamCreate
    {
        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống")]
        [StringLength(40, ErrorMessage = "Tên loại sản phẩm tối đa 40 ký tự")]
        public string TenLoaiSanPham { get; set; }

        [StringLength(50, ErrorMessage = "Kí hiệu tối đa 50 ký tự")] // Tùy chỉnh độ dài nếu cần
        public string? KiHieu { get; set; } // Nullable, không bắt buộc
    }
}

