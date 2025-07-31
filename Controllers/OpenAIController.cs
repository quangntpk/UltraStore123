using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly IOpenAIServices _openAIServices;

        public OpenAIController(IOpenAIServices openAIServices)
        {
            _openAIServices = openAIServices ?? throw new ArgumentNullException(nameof(openAIServices));
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Truy vấn không được để trống.");
            }

            try
            {
                var response = await _openAIServices.GetChatResponseAsync(request.Query);
                return Ok(new { Response = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }
    }

    public class ChatRequest
    {
        public string Query { get; set; }
    }
}