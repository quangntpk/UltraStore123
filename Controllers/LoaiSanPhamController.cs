using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Threading.Tasks;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiSanPhamController : ControllerBase
    {
        private readonly ILoaiSanPhamServices _loaiSanPhamServices;

        public LoaiSanPhamController(ILoaiSanPhamServices loaiSanPhamServices)
        {
            _loaiSanPhamServices = loaiSanPhamServices ?? throw new ArgumentNullException(nameof(loaiSanPhamServices));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLoaiSanPham([FromQuery] int? trangThai = null)
        {
            try
            {
                var list = await _loaiSanPhamServices.GetAllLoaiSanPhamAsync(trangThai);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lấy danh sách loại sản phẩm.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoaiSanPham(int id)
        {
            try
            {
                var loaiSanPham = await _loaiSanPhamServices.GetLoaiSanPhamAsync(id);
                return Ok(loaiSanPham);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi lấy thông tin loại sản phẩm.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoaiSanPham([FromBody] LoaiSanPhamCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdLoaiSanPham = await _loaiSanPhamServices.CreateLoaiSanPhamAsync(model);
                return CreatedAtAction(nameof(GetLoaiSanPham), new { id = createdLoaiSanPham.MaLoaiSanPham }, createdLoaiSanPham);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi tạo loại sản phẩm.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoaiSanPham(int id, [FromBody] LoaiSanPhamEdit model)
        {
            if (id != model.MaLoaiSanPham)
                return BadRequest(new { error = "Mã loại sản phẩm trong URL không khớp với dữ liệu." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedLoaiSanPham = await _loaiSanPhamServices.UpdateLoaiSanPhamAsync(model);
                return Ok(updatedLoaiSanPham);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi cập nhật loại sản phẩm.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoaiSanPham(int id)
        {
            try
            {
                var result = await _loaiSanPhamServices.DeleteLoaiSanPhamAsync(id);
                if (!result)
                    return NotFound(new { error = $"Loại sản phẩm với mã '{id}' không tồn tại." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi xóa loại sản phẩm.", details = ex.Message });
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchLoaiSanPham([FromQuery] string? tenLoai, [FromQuery] string? kiHieu, [FromQuery] int? trangThai = null)
        {
            try
            {
                var result = await _loaiSanPhamServices.SearchLoaiSanPhamAsync(tenLoai, kiHieu, trangThai);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi khi tìm kiếm loại sản phẩm.", details = ex.Message });
            }
        }
    }
}

