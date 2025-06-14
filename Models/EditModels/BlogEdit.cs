namespace UltraStrore.Models.EditModels
{
    public class BlogEdit
    {
        public int? MaBlog { get; set; }
        public string? MaNguoiDung { get; set; }

        public DateTime? NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }

        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }

        // SEO-friendly fields
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        public byte[]? HinhAnh { get; set; }
        public string? MoTaHinhAnh { get; set; } // alt text

        public bool IsPublished { get; set; }

        public List<string>? Tags { get; set; }
    }
}
