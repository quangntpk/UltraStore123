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
                HoTen = null, // Cần logic lấy HoTen từ bảng người dùng nếu có
                NgayTao = b.NgayTao,
                NoiDung = b.NoiDung,
                TieuDe = b.TieuDe,
                HinhAnh = b.HinhAnh != null ? Convert.ToBase64String(b.HinhAnh) : null
            })
            .ToList();
    }

    public async Task<BlogView> CreateBlog(BlogCreate blogCreate)
    {
        byte[]? hinhAnh = null;
        if (!string.IsNullOrEmpty(blogCreate.HinhAnh))
        {
            try
            {
                hinhAnh = Convert.FromBase64String(blogCreate.HinhAnh);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Chuỗi Base64 không hợp lệ.");
            }
        }

        var blog = new Blogs
        {
            MaNguoiDung = blogCreate.MaNguoiDung,
            NgayTao = blogCreate.NgayTao ?? DateTime.Now,
            NoiDung = blogCreate.NoiDung,
            TieuDe = blogCreate.TieuDe,
            HinhAnh = hinhAnh
        };

        _context.Blogs.Add(blog);
        await _context.SaveChangesAsync();

        return new BlogView
        {
            MaBlog = blog.MaBlog,
            MaNguoiDung = blog.MaNguoiDung,
            HoTen = null, // Cần logic lấy HoTen từ bảng người dùng nếu có
            NgayTao = blog.NgayTao,
            NoiDung = blog.NoiDung,
            TieuDe = blog.TieuDe,
        };
    }

    public async Task<BlogView> EditBlog(BlogEdit blogEdit)
    {
        var blog = await _context.Blogs.FindAsync(blogEdit.MaBlog);
        if (blog == null)
            return null;

        blog.MaNguoiDung = blogEdit.MaNguoiDung ?? blog.MaNguoiDung;
        blog.NgayTao = blogEdit.NgayTao ?? blog.NgayTao;
        blog.NoiDung = blogEdit.NoiDung ?? blog.NoiDung;
        blog.TieuDe = blogEdit.TieuDe ?? blog.TieuDe;

        if (!string.IsNullOrEmpty(blogEdit.HinhAnh))
        {
            try
            {
                blog.HinhAnh = Convert.FromBase64String(blogEdit.HinhAnh);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Chuỗi Base64 không hợp lệ.");
            }
        }
        else if (blogEdit.HinhAnh == null)
        {
            blog.HinhAnh = null; // Xóa hình ảnh nếu HinhAnh là null
        }

        _context.Blogs.Update(blog);
        await _context.SaveChangesAsync();

        return new BlogView
        {
            MaBlog = blog.MaBlog,
            MaNguoiDung = blog.MaNguoiDung,
            HoTen = null, // Cần logic lấy HoTen từ bảng người dùng nếu có
            NgayTao = blog.NgayTao,
            NoiDung = blog.NoiDung,
            TieuDe = blog.TieuDe,
            HinhAnh = blog.HinhAnh != null ? Convert.ToBase64String(blog.HinhAnh) : null
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
}