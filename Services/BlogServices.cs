using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UltraStrore.Repository
{
    public class BlogServices : IBlogServices
    {
        private readonly ApplicationDbContext _context;

        public BlogServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<BlogView> GetAllBlogs()
        {
            return _context.Blogs
                .Select(b => new BlogView
                {
                    MaBlog = b.MaBlog,
                    MaNguoiDung = b.MaNguoiDung,
                    HoTen = null,
                    NgayTao = b.NgayTao,
                    NgayCapNhat = b.NgayCapNhat,
                    TieuDe = b.TieuDe,
                    NoiDung = b.NoiDung,
                    Slug = b.Slug,
                    MetaTitle = b.MetaTitle,
                    MetaDescription = b.MetaDescription,
                    HinhAnh = b.HinhAnh,
                    MoTaHinhAnh = b.MoTaHinhAnh,
                    IsPublished = b.IsPublished,
                    Tags = b.Tags,
                    Likes = b.Likes,
                    UserLikes = b.UserLikes
                })
                .ToList();
        }

        public async Task<BlogView> CreateBlog(BlogCreate blogCreate)
        {
            var blog = new Blogs
            {
                MaNguoiDung = blogCreate.MaNguoiDung,
                NgayTao = blogCreate.NgayTao ?? DateTime.Now,
                NgayCapNhat = DateTime.Now,
                TieuDe = blogCreate.TieuDe,
                NoiDung = blogCreate.NoiDung,
                Slug = blogCreate.Slug ?? GenerateSlug(blogCreate.TieuDe),
                MetaTitle = blogCreate.MetaTitle,
                MetaDescription = blogCreate.MetaDescription,
                HinhAnh = blogCreate.HinhAnh,
                MoTaHinhAnh = blogCreate.MoTaHinhAnh,
                IsPublished = blogCreate.IsPublished,
                Tags = blogCreate.Tags,
                Likes = 0,
                UserLikes = new List<string>()
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return new BlogView
            {
                MaBlog = blog.MaBlog,
                MaNguoiDung = blog.MaNguoiDung,
                HoTen = null,
                NgayTao = blog.NgayTao,
                NgayCapNhat = blog.NgayCapNhat,
                TieuDe = blog.TieuDe,
                NoiDung = blog.NoiDung,
                Slug = blog.Slug,
                MetaTitle = blog.MetaTitle,
                MetaDescription = blog.MetaDescription,
                HinhAnh = blog.HinhAnh,
                MoTaHinhAnh = blog.MoTaHinhAnh,
                IsPublished = blog.IsPublished,
                Tags = blog.Tags,
                Likes = blog.Likes,
                UserLikes = blog.UserLikes
            };
        }

        public async Task<BlogView?> EditBlog(BlogEdit blogEdit)
        {
            var blog = await _context.Blogs.FindAsync(blogEdit.MaBlog);
            if (blog == null)
                return null;

            blog.MaNguoiDung = blogEdit.MaNguoiDung ?? blog.MaNguoiDung;
            blog.TieuDe = blogEdit.TieuDe ?? blog.TieuDe;
            blog.NoiDung = blogEdit.NoiDung ?? blog.NoiDung;
            blog.NgayCapNhat = DateTime.Now;
            blog.Slug = blogEdit.Slug ?? blog.Slug ?? GenerateSlug(blogEdit.TieuDe ?? blog.TieuDe);
            blog.MetaTitle = blogEdit.MetaTitle ?? blog.MetaTitle;
            blog.MetaDescription = blogEdit.MetaDescription ?? blog.MetaDescription;
            blog.HinhAnh = blogEdit.HinhAnh ?? blog.HinhAnh;
            blog.MoTaHinhAnh = blogEdit.MoTaHinhAnh ?? blog.MoTaHinhAnh;
            blog.IsPublished = blogEdit.IsPublished;
            blog.Tags = blogEdit.Tags ?? blog.Tags;

            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();

            return new BlogView
            {
                MaBlog = blog.MaBlog,
                MaNguoiDung = blog.MaNguoiDung,
                HoTen = null,
                NgayTao = blog.NgayTao,
                NgayCapNhat = blog.NgayCapNhat,
                TieuDe = blog.TieuDe,
                NoiDung = blog.NoiDung,
                Slug = blog.Slug,
                MetaTitle = blog.MetaTitle,
                MetaDescription = blog.MetaDescription,
                HinhAnh = blog.HinhAnh,
                MoTaHinhAnh = blog.MoTaHinhAnh,
                IsPublished = blog.IsPublished,
                Tags = blog.Tags,
                Likes = blog.Likes,
                UserLikes = blog.UserLikes
            };
        }

        public async Task<bool> DeleteBlog(int maBlog)
        {
            var blog = await _context.Blogs.FindAsync(maBlog);
            if (blog == null)
                return false;

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BlogView?> GetBlogById(int maBlog)
        {
            var blog = await _context.Blogs.FindAsync(maBlog);
            if (blog == null)
                return null;

            return new BlogView
            {
                MaBlog = blog.MaBlog,
                MaNguoiDung = blog.MaNguoiDung,
                HoTen = null,
                NgayTao = blog.NgayTao,
                NgayCapNhat = blog.NgayCapNhat,
                TieuDe = blog.TieuDe,
                NoiDung = blog.NoiDung,
                Slug = blog.Slug,
                MetaTitle = blog.MetaTitle,
                MetaDescription = blog.MetaDescription,
                HinhAnh = blog.HinhAnh,
                MoTaHinhAnh = blog.MoTaHinhAnh,
                IsPublished = blog.IsPublished,
                Tags = blog.Tags,
                Likes = blog.Likes,
                UserLikes = blog.UserLikes
            };
        }

        public async Task<BlogView?> GetBlogBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty", nameof(slug));

            var blog = await _context.Blogs.AsNoTracking().FirstOrDefaultAsync(b => b.Slug == slug);
            if (blog == null)
                return null;

            return new BlogView
            {
                MaBlog = blog.MaBlog,
                MaNguoiDung = blog.MaNguoiDung,
                HoTen = null,
                NgayTao = blog.NgayTao,
                NgayCapNhat = blog.NgayCapNhat,
                TieuDe = blog.TieuDe,
                NoiDung = blog.NoiDung,
                Slug = blog.Slug,
                MetaTitle = blog.MetaTitle,
                MetaDescription = blog.MetaDescription,
                HinhAnh = blog.HinhAnh,
                MoTaHinhAnh = blog.MoTaHinhAnh,
                IsPublished = blog.IsPublished,
                Tags = blog.Tags,
                Likes = blog.Likes,
                UserLikes = blog.UserLikes
            };
        }

        public async Task<BlogView> LikeBlog(int maBlog, string maNguoiDung)
        {
            var blog = await _context.Blogs.FindAsync(maBlog);
            if (blog == null)
                throw new ArgumentException("Blog not found", nameof(maBlog));

            if (string.IsNullOrWhiteSpace(maNguoiDung))
                throw new ArgumentException("User ID cannot be empty", nameof(maNguoiDung));

            if (blog.UserLikes == null)
                blog.UserLikes = new List<string>();

            if (!blog.UserLikes.Contains(maNguoiDung))
            {
                blog.UserLikes.Add(maNguoiDung);
                blog.Likes++;
                _context.Update(blog);
                await _context.SaveChangesAsync();
            }

            return new BlogView
            {
                MaBlog = blog.MaBlog,
                MaNguoiDung = blog.MaNguoiDung,
                HoTen = null,
                NgayTao = blog.NgayTao,
                NgayCapNhat = blog.NgayCapNhat,
                TieuDe = blog.TieuDe,
                NoiDung = blog.NoiDung,
                Slug = blog.Slug,
                MetaTitle = blog.MetaTitle,
                MetaDescription = blog.MetaDescription,
                HinhAnh = blog.HinhAnh,
                MoTaHinhAnh = blog.MoTaHinhAnh,
                IsPublished = blog.IsPublished,
                Tags = blog.Tags,
                Likes = blog.Likes,
                UserLikes = blog.UserLikes
            };
        }

        public async Task<BlogView> UnlikeBlog(int maBlog, string maNguoiDung)
        {
            var blog = await _context.Blogs.FindAsync(maBlog);
            if (blog == null)
                throw new ArgumentException("Blog not found", nameof(maBlog));

            if (string.IsNullOrWhiteSpace(maNguoiDung))
                throw new ArgumentException("User ID cannot be empty", nameof(maNguoiDung));

            if (blog.UserLikes != null && blog.UserLikes.Contains(maNguoiDung))
            {
                blog.UserLikes.Remove(maNguoiDung);
                blog.Likes--;
                _context.Update(blog);
                await _context.SaveChangesAsync();
            }

            return new BlogView
            {
                MaBlog = blog.MaBlog,
                MaNguoiDung = blog.MaNguoiDung,
                HoTen = null,
                NgayTao = blog.NgayTao,
                NgayCapNhat = blog.NgayCapNhat,
                TieuDe = blog.TieuDe,
                NoiDung = blog.NoiDung,
                Slug = blog.Slug,
                MetaTitle = blog.MetaTitle,
                MetaDescription = blog.MetaDescription,
                HinhAnh = blog.HinhAnh,
                MoTaHinhAnh = blog.MoTaHinhAnh,
                IsPublished = blog.IsPublished,
                Tags = blog.Tags,
                Likes = blog.Likes,
                UserLikes = blog.UserLikes
            };
        }

        private string GenerateSlug(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Guid.NewGuid().ToString();

            var slug = title.ToLower().Trim()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace("–", "-");

            return slug;
        }
    }
}