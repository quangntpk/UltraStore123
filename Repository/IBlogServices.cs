using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace UltraStrore.Repository
{
    public interface IBlogServices
    {
        List<BlogView> GetAllBlogs();
        Task<BlogView> CreateBlog(BlogCreate blogCreate);
        Task<BlogView> EditBlog(BlogEdit blogEdit);
        Task<bool> DeleteBlog(int maBlog);
        Task<BlogView?> GetBlogById(int maBlog);
        Task<BlogView> GetBlogBySlug(string slug);
        Task<BlogView> LikeBlog(int maBlog, string maNguoiDung);
        Task<BlogView> UnlikeBlog(int maBlog, string maNguoiDung);

    }
}
