using UltraStrore.Data;
namespace UltraStrore.Models.CreateModels
{
    public class BlogCreate
    {
        public int? MaBlog { get; set; }
        public string? MaNguoiDung { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? NoiDung { get; set; }
        public string? TieuDe { get; set; }
        public string? HinhAnh { get; set; }
    }
}
