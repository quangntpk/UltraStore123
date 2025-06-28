using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogServices _blogServices;

        public BlogController(IBlogServices blogServices)
        {
            _blogServices = blogServices;
        }

        [HttpGet]
        public ActionResult<List<BlogView>> GetAllBlogs()
        {
            try
            {
                var blogs = _blogServices.GetAllBlogs();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            var blog = await _blogServices.GetBlogById(id);
            if (blog == null)
                return NotFound();
            return Ok(blog);
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBlogBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest("Slug cannot be empty");

            var blog = await _blogServices.GetBlogBySlug(slug);
            if (blog == null)
                return NotFound();
            return Ok(blog);
        }

        [HttpPost("CreateBlog")]
        public async Task<ActionResult<BlogView>> CreateBlog([FromBody] BlogCreate blogCreate)
        {
            try
            {
                var blog = await _blogServices.CreateBlog(blogCreate);
                return Ok(blog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPut("{maBlog}")]
        public async Task<ActionResult<BlogView>> EditBlog(int maBlog, [FromBody] BlogEdit blogEdit)
        {
            try
            {
                if (maBlog != blogEdit.MaBlog)
                    return BadRequest("Mã blog không khớp.");

                var blog = await _blogServices.EditBlog(blogEdit);
                if (blog == null)
                    return NotFound("Blog không tồn tại.");

                return Ok(blog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpDelete("{maBlog}")]
        public async Task<ActionResult> DeleteBlog(int maBlog)
        {
            try
            {
                var result = await _blogServices.DeleteBlog(maBlog);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound("Blog không tồn tại.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
