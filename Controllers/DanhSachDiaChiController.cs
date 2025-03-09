using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhSachDiaChiController : ControllerBase
    {
        private readonly IDanhSachDiaChiServices _services;
        public DanhSachDiaChiController(IDanhSachDiaChiServices services)
        {
            _services = services;
        }

        // GET: api/DanhSachDiaChi?searchTerm=xxx
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm)
        {
            var list = await _services.GetDanhSachDiaChiList(searchTerm);
            return Ok(list);
        }

        // GET: api/DanhSachDiaChi/maNguoiDung/{maNguoiDung}
        [HttpGet("maNguoiDung/{maNguoiDung}")]
        public async Task<IActionResult> GetByMaNguoiDung(string maNguoiDung)
        {
            var list = await _services.GetDanhSachDiaChiByMaNguoiDung(maNguoiDung);
            return Ok(list);
        }

        // GET: api/DanhSachDiaChi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var address = await _services.GetDanhSachDiaChiById(id);
                return Ok(address);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/DanhSachDiaChi
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DanhSachDiaChiCreate model)
        {
            try
            {
                var newAddress = await _services.CreateDanhSachDiaChi(model);
                return CreatedAtAction(nameof(GetById), new { id = newAddress.MaDiaChi }, newAddress);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/DanhSachDiaChi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DanhSachDiaChiEdit model)
        {
            if (id != model.MaDiaChi)
                return BadRequest("Mã địa chỉ không khớp");

            try
            {
                var updatedAddress = await _services.UpdateDanhSachDiaChi(model);
                return Ok(updatedAddress);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE: api/DanhSachDiaChi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _services.DeleteDanhSachDiaChi(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
