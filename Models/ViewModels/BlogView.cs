namespace UltraStrore.Models.ViewModels
{
    public class BlogView
    {
        public int? MaBlog { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? HoTen { get; set; }

        public DateTime? NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }

        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }

        // Các trường SEO
        public string? Slug { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        public byte[]? HinhAnh { get; set; }
        public string? MoTaHinhAnh { get; set; }

        public bool IsPublished { get; set; }

        public List<string>? Tags { get; set; }
        public int Likes { get; set; }
        public List<string>? UserLikes { get; set; }
    }
}
