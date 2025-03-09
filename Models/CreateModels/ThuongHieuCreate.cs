using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.CreateModels
{
    public class ThuongHieuCreate
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(40, ErrorMessage = "Tên thương hiệu tối đa 40 ký tự")]
        public string TenThuongHieu { get; set; }
    }
}
