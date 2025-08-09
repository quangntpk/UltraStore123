using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Repository;
using System.Threading.Tasks;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinNhanController : ControllerBase
    {
        private readonly ITinNhanServices _services;

        public TinNhanController(ITinNhanServices services)
        {
            _services = services;
        }

        [HttpPost("gui")]
        public async Task<IActionResult> GuiTinNhan([FromForm] TinNhanCreate model)
        {
            if (model == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            try
            {
                var result = await _services.GuiTinNhanAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi tin nhắn: {ex.Message}");
                return StatusCode(500, "Lỗi server khi gửi tin nhắn");
            }
        }

        [HttpGet("doan-chat")]
        public async Task<IActionResult> LayTinNhan([FromQuery] string nguoiGuiId, [FromQuery] string nguoiNhanId)
        {
            if (string.IsNullOrEmpty(nguoiGuiId) || string.IsNullOrEmpty(nguoiNhanId))
            {
                return BadRequest("Ngươi gửi và người nhận không được để trống");
            }

            try
            {
                var result = await _services.LayTinNhanGiuaHaiNguoiAsync(nguoiGuiId, nguoiNhanId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy tin nhắn: {ex.Message}");
                return StatusCode(500, "Lỗi server khi lấy tin nhắn");
            }
        }

        [HttpGet("threads")]
        public async Task<IActionResult> LayThreads([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId không được để trống");
            }

            try
            {
                var result = await _services.LayDanhSachThreadsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách threads: {ex.Message}");
                return StatusCode(500, "Lỗi server khi lấy danh sách threads");
            }
        }
    }
}