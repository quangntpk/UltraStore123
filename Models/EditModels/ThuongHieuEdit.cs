using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.EditModels
{
    public class ThuongHieuEdit
    {
        [Required]
        public int MaThuongHieu { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(40, ErrorMessage = "Tên thương hiệu tối đa 40 ký tự")]
        public string TenThuongHieu { get; set; }
    }
}
