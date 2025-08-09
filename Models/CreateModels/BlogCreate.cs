using UltraStrore.Data;
namespace UltraStrore.Models.CreateModels
{
    public class BlogCreate
    {
        public int? MaBlog { get; set; }
        public string? MaNguoiDung { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }

        // Chuẩn SEO
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        public byte[]? HinhAnh { get; set; }
        public string? MoTaHinhAnh { get; set; } // alt ảnh
        public string? ChuDe { get; set; }
        public bool IsPublished { get; set; } = false;

        public List<string>? Tags { get; set; } // danh sách từ khóa


    }
}
