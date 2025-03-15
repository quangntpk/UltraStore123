using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltraStrore.Data;
using System.Linq;
using System.Threading.Tasks;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

[Route("api/[controller]")]
[ApiController]
public class LoaiSanPhamController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LoaiSanPhamController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/LoaiSanPham?keyword={keyword}
    [HttpGet]
    public async Task<IActionResult> GetLoaiSanPham(string keyword = "")
    {
        var loaiSanPham = await _context.LoaiSanPhams
            .Where(l => string.IsNullOrEmpty(keyword) || l.TenLoaiSanPham.Contains(keyword))
            .Select(l => new LoaiSanPhamView
            {
                MaLoaiSanPham = l.MaLoaiSanPham,
                TenLoaiSanPham = l.TenLoaiSanPham
            })
            .ToListAsync();

        return Ok(loaiSanPham);
    }

    // POST: api/LoaiSanPham
    [HttpPost]
    public async Task<IActionResult> PostLoaiSanPham([FromBody] LoaiSanPhamCreate model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var loaiSanPham = new LoaiSanPham { TenLoaiSanPham = model.TenLoaiSanPham };
        _context.LoaiSanPhams.Add(loaiSanPham);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLoaiSanPham), new { id = loaiSanPham.MaLoaiSanPham }, loaiSanPham);
    }

    // PUT: api/LoaiSanPham/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutLoaiSanPham(int id, [FromBody] LoaiSanPhamEdit model)
    {
        if (!ModelState.IsValid || id != model.MaLoaiSanPham)
            return BadRequest(ModelState);

        var loaiSanPham = await _context.LoaiSanPhams.FindAsync(id);
        if (loaiSanPham == null)
            return NotFound();

        loaiSanPham.TenLoaiSanPham = model.TenLoaiSanPham;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/LoaiSanPham/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLoaiSanPham(int id)
    {
        var loaiSanPham = await _context.LoaiSanPhams.FindAsync(id);
        if (loaiSanPham == null)
            return NotFound();

        // Kiểm tra khóa ngoại (nếu có sản phẩm liên quan)
        if (await _context.SanPhams.AnyAsync(s => s.MaLoaiSanPham == id))
            return BadRequest("Không thể xóa loại sản phẩm vì có sản phẩm liên quan.");

        _context.LoaiSanPhams.Remove(loaiSanPham);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}