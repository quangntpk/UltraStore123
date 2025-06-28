using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.DTOs;
using UltraStrore.Repository;

using UltraStrore.Services;


namespace UltraStrore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IOrderNotificationService _service;
        public EmailController(IOrderNotificationService service)
        {
            _service = service;
        }

        [HttpPost("SendWithQr")]
        public async Task<IActionResult> SendEmailWithQr([FromBody] EmailOrderDto dto)
        {
            await _service.SendOrderStatusNotificationFromFrontendAsync(dto);
            return Ok(new { message = "Email đã được gửi kèm mã QR." });
        }
    }

}
