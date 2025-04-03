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
            var list = await _thuongHieuServices.GetAllThuongHieuAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetThuongHieu(int id)
        {
            try
            {
                var thuongHieu = await _thuongHieuServices.GetThuongHieuAsync(id);
                return Ok(thuongHieu);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Thương hiệu không tồn tại.");
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
            catch (KeyNotFoundException)
            {
                return NotFound("Thương hiệu không tồn tại.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThuongHieu(int id)
        {
            var result = await _thuongHieuServices.DeleteThuongHieuAsync(id);
            return result ? NoContent() : NotFound("Thương hiệu không tồn tại.");
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchThuongHieu([FromQuery] string tenThuongHieu)
        {
            var result = await _thuongHieuServices.SearchThuongHieuAsync(tenThuongHieu);
            return Ok(result);
        }

        [HttpGet("{id}/SanPham")]
        public async Task<IActionResult> GetSanPhamByThuongHieu(int id)
        {
            try
            {
                var sanPhams = await _thuongHieuServices.GetSanPhamByThuongHieuAsync(id);
                return Ok(sanPhams);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Thương hiệu không tồn tại.");
            }
        }
    }
}