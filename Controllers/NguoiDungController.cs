using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Utils; // Để sử dụng PasswordHasher

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly INguoiDungServices _services;
        public NguoiDungController(INguoiDungServices services)
        {
            _services = services;
        }

        // GET: api/NguoiDung?searchTerm=xxx
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm)
        {
            // Lấy danh sách người dùng, lọc theo searchTerm nếu có
            var list = await _services.GetNguoiDungList(searchTerm);
            return Ok(list);
        }

        // GET: api/NguoiDung/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                // Lấy thông tin người dùng theo ID
                var user = await _services.GetNguoiDungById(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/NguoiDung
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NguoiDungCreate model)
        {
            try
            {
                // Nếu mật khẩu được cung cấp, mã hóa mật khẩu trước khi tạo người dùng
                if (!string.IsNullOrEmpty(model.MatKhau))
                {
                    model.MatKhau = PasswordHasher.HashPassword(model.MatKhau);
                }
                var createdUser = await _services.CreateNguoiDung(model);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.MaNguoiDung }, createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/NguoiDung/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] NguoiDungEdit model)
        {
            if (id != model.MaNguoiDung)
                return BadRequest("Mã người dùng không khớp");

            try
            {
                // Cập nhật thông tin người dùng theo model chỉnh sửa
                var updatedUser = await _services.UpdateNguoiDung(model);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE: api/NguoiDung/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _services.DeleteNguoiDung(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
