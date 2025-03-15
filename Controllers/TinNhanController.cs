using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

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

        [HttpPost("Send")]
        public async Task<IActionResult> SendMessage([FromBody] TinNhanCreate model)
        {
            if (model == null)
                return BadRequest("Model không được để trống.");

            try
            {
                var result = await _services.SendMessageAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpGet("Conversation")]
        public async Task<IActionResult> GetConversation([FromQuery] string nguoiGuiId, [FromQuery] string nguoiNhanId)
        {
            if (string.IsNullOrWhiteSpace(nguoiGuiId) || string.IsNullOrWhiteSpace(nguoiNhanId))
                return BadRequest("Cần cung cấp đủ ID người gửi và người nhận.");

            try
            {
                var conversation = await _services.GetConversationAsync(nguoiGuiId, nguoiNhanId);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] TinNhanEdit model)
        {
            if (id != model.MaTinNhan)
                return BadRequest("ID tin nhắn không khớp.");

            try
            {
                var updatedMessage = await _services.UpdateMessageAsync(model);
                return Ok(updatedMessage);
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                bool result = await _services.DeleteMessageAsync(id);
                if (!result)
                    return NotFound("Tin nhắn không tồn tại.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }

        [HttpPut("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NguoiGuiId) || string.IsNullOrWhiteSpace(request.NguoiNhanId))
                return BadRequest("Cần cung cấp ID người gửi và người nhận.");

            try
            {
                await _services.MarkAsReadAsync(request.NguoiGuiId, request.NguoiNhanId);
                return Ok("Tin nhắn đã được đánh dấu là đã đọc.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Có lỗi xảy ra: {ex.Message}");
            }
        }
    }
}
