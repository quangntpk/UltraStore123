using Microsoft.AspNetCore.Mvc;
using UltraStrore.Utils;
using UltraStrore.Services;
using UltraStrore.Repository;
using UltraStrore.Models.CreateModels;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiServices service;
        public GeminiController(IGeminiServices service)
        {
            this.service = service;
        }
        [HttpGet("TraLoi")]
        public async Task<IActionResult> TraLoi(string question)
        {
            var data = await this.service.TraLoi(question);
            return Ok(data);
        }
        [HttpPost("Response")]
        public async Task<IActionResult> Response(RequestGeminiHinhAnh info)            
        {
            var data = await this.service.Response(info);
            return Ok(data);
        }
        [HttpGet("PhanLoaiGopY")]
        public async Task<IActionResult> PhanLoaiGopY(string noiDung)
        {
            var data = await this.service.PhanLoaiGopY(noiDung);
            return Ok(data);
        }
        [HttpGet("SmartAI")]
        public async Task<IActionResult> SmartAI(string noiDung)
        {
            var data = await this.service.TraLoiUpgrade(noiDung);
            return Ok(data);
        }
    }
}
