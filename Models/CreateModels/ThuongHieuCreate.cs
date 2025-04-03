using System.ComponentModel.DataAnnotations;

namespace UltraStrore.Models.CreateModels
{
    public class ThuongHieuCreate
    {
        public int MaThuongHieu { get; set; }
        public string TenThuongHieu { get; set; }
        public byte[]? HinhAnh { get; set; }
    }
}
