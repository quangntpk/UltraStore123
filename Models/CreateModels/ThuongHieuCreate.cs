using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.CreateModels
{
    public class ThuongHieuCreate
    {
        public string TenThuongHieu { get; set; }
        public byte[]? HinhAnh { get; set; }
        public int? TrangThai { get; set; }
    }
}
