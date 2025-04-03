using Microsoft.AspNetCore.Mvc;
using UltraStrore.Repository;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiSanPhamController : ControllerBase
    {
        private readonly ILoaiSanPhamServices _loaiSanPhamServices;

        public LoaiSanPhamController(ILoaiSanPhamServices loaiSanPhamServices)
        {
            _loaiSanPhamServices = loaiSanPhamServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLoaiSanPham()
        {
            try
            {
                var list = await _loaiSanPhamServices.GetAllLoaiSanPhamAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoaiSanPham([FromBody] LoaiSanPhamCreate model)
        {
            try
            {
                var createdLoaiSanPham = await _loaiSanPhamServices.CreateLoaiSanPhamAsync(model);
                return CreatedAtAction(nameof(GetLoaiSanPham), new { id = createdLoaiSanPham.MaLoaiSanPham }, createdLoaiSanPham);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoaiSanPham(int id, [FromBody] LoaiSanPhamEdit model)
        {
            if (id != model.MaLoaiSanPham)
                return BadRequest("Mã loại sản phẩm không hợp lệ.");

            try
            {
                var updatedLoaiSanPham = await _loaiSanPhamServices.UpdateLoaiSanPhamAsync(model);
                return Ok(updatedLoaiSanPham);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoaiSanPham(int id)
        {
            try
            {
                var result = await _loaiSanPhamServices.DeleteLoaiSanPhamAsync(id);
                if (!result)
                    return NotFound("Loại sản phẩm không tồn tại.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/SanPham")]
        public async Task<IActionResult> GetSanPhamByLoai(int id)
        {
            try
            {
                var sanPhams = await _loaiSanPhamServices.GetSanPhamByLoaiAsync(id);
                return Ok(sanPhams);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchLoaiSanPham([FromQuery] string? tenLoai, [FromQuery] string? kiHieu)
        {
            try
            {
                var result = await _loaiSanPhamServices.SearchLoaiSanPhamAsync(tenLoai, kiHieu);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}