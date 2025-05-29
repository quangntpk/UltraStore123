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
            var result = await _services.GuiTinNhanAsync(model);
            return Ok(result);
        }

        [HttpGet("doan-chat")]
        public async Task<IActionResult> LayTinNhan([FromQuery] string nguoiGuiId, [FromQuery] string nguoiNhanId)
        {
            var result = await _services.LayTinNhanGiuaHaiNguoiAsync(nguoiGuiId, nguoiNhanId);
            return Ok(result);
        }

        [HttpGet("threads")]
        public async Task<IActionResult> LayThreads([FromQuery] string userId)
        {
            var result = await _services.LayDanhSachThreadsAsync(userId);
            return Ok(result);
        }
    }
}
