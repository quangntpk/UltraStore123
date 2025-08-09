using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace UltraStrore.Controllers
{
    [Authorize(Roles = "admin,staff")]
    [Route("api/[controller]")]
    [ApiController]
    public class ThuongHieuController : ControllerBase
    {
        private readonly IThuongHieuServices _thuongHieuServices;

        public ThuongHieuController(IThuongHieuServices thuongHieuServices)
        {
            _thuongHieuServices = thuongHieuServices ?? throw new ArgumentNullException(nameof(thuongHieuServices));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllThuongHieu()
        {
            try
            {
                var list = await _thuongHieuServices.GetAllThuongHieuAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetThuongHieu(int id)
        {
            try
            {
                var thuongHieu = await _thuongHieuServices.GetThuongHieuAsync(id);
                return Ok(thuongHieu);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateThuongHieu([FromBody] ThuongHieuCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdThuongHieu = await _thuongHieuServices.CreateThuongHieuAsync(model);
                return CreatedAtAction(nameof(GetThuongHieu), new { id = createdThuongHieu.MaThuongHieu }, createdThuongHieu);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateThuongHieu(int id, [FromBody] ThuongHieuEdit model)
        {
            if (id != model.MaThuongHieu)
                return BadRequest("Mã thương hiệu không khớp.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedThuongHieu = await _thuongHieuServices.UpdateThuongHieuAsync(model);
                return Ok(updatedThuongHieu);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThuongHieu(int id)
        {
            try
            {
                var result = await _thuongHieuServices.DeleteThuongHieuAsync(id);
                return result ? NoContent() : NotFound("Thương hiệu không tồn tại.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchThuongHieu([FromQuery] string tenThuongHieu)
        {
            try
            {
                var result = await _thuongHieuServices.SearchThuongHieuAsync(tenThuongHieu);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpGet("{id}/SanPham")]
        public async Task<IActionResult> GetSanPhamByThuongHieu(int id)
        {
            try
            {
                var sanPhams = await _thuongHieuServices.GetSanPhamByThuongHieuAsync(id);
                return Ok(sanPhams);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }
    }
}
