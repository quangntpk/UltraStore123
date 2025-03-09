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
public class ThuongHieuController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ThuongHieuController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ThuongHieu?keyword={keyword}
    [HttpGet]
    public async Task<IActionResult> GetThuongHieu(string keyword = "")
    {
        var thuongHieu = await _context.ThuongHieus
            .Where(t => string.IsNullOrEmpty(keyword) || t.TenThuongHieu.Contains(keyword))
            .Select(t => new ThuongHieuView
            {
                MaThuongHieu = t.MaThuongHieu,
                TenThuongHieu = t.TenThuongHieu
            })
            .ToListAsync();

        return Ok(thuongHieu);
    }

    // POST: api/ThuongHieu
    [HttpPost]
    public async Task<IActionResult> PostThuongHieu([FromBody] ThuongHieuCreate model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var thuongHieu = new ThuongHieu { TenThuongHieu = model.TenThuongHieu };
        _context.ThuongHieus.Add(thuongHieu);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetThuongHieu), new { id = thuongHieu.MaThuongHieu }, thuongHieu);
    }

    // PUT: api/ThuongHieu/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutThuongHieu(int id, [FromBody] ThuongHieuEdit model)
    {
        if (!ModelState.IsValid || id != model.MaThuongHieu)
            return BadRequest(ModelState);

        var thuongHieu = await _context.ThuongHieus.FindAsync(id);
        if (thuongHieu == null)
            return NotFound();

        thuongHieu.TenThuongHieu = model.TenThuongHieu;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/ThuongHieu/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteThuongHieu(int id)
    {
        var thuongHieu = await _context.ThuongHieus.FindAsync(id);
        if (thuongHieu == null)
            return NotFound();

        // Kiểm tra khóa ngoại (nếu có sản phẩm liên quan)
        if (await _context.SanPhams.AnyAsync(s => s.MaThuongHieu == id))
            return BadRequest("Không thể xóa thương hiệu vì có sản phẩm liên quan.");

        _context.ThuongHieus.Remove(thuongHieu);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}