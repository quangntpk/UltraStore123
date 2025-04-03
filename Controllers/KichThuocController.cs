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
    public class KichThuocController : ControllerBase
    {
        private readonly IKichThuocServices _kichThuocServices;

        public KichThuocController(IKichThuocServices kichThuocServices)
        {
            _kichThuocServices = kichThuocServices ?? throw new ArgumentNullException(nameof(kichThuocServices));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKichThuoc()
        {
            var list = await _kichThuocServices.GetAllKichThuocAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKichThuoc(int id)
        {
            try
            {
                var kichThuoc = await _kichThuocServices.GetKichThuocAsync(id);
                return Ok(kichThuoc);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Kích thước không tồn tại.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateKichThuoc([FromBody] KichThuocCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdKichThuoc = await _kichThuocServices.CreateKichThuocAsync(model);
                return CreatedAtAction(nameof(GetKichThuoc), new { id = createdKichThuoc.MaKichThuoc }, createdKichThuoc);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKichThuoc(int id, [FromBody] KichThuocEdit model)
        {
            if (id != model.MaKichThuoc)
                return BadRequest("Mã kích thước không khớp.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedKichThuoc = await _kichThuocServices.UpdateKichThuocAsync(model);
                return Ok(updatedKichThuoc);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Kích thước không tồn tại.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKichThuoc(int id)
        {
            var result = await _kichThuocServices.DeleteKichThuocAsync(id);
            return result ? NoContent() : NotFound("Kích thước không tồn tại.");
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchKichThuoc([FromQuery] string tenKichThuoc)
        {
            var result = await _kichThuocServices.SearchKichThuocAsync(tenKichThuoc);
            return Ok(result);
        }
    }
}