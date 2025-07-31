using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Repository;
using UltraStrore.Utils;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly IOpenAIServices _service;

        public OpenAIController(IOpenAIServices service)
        {
            _service = service;
        }

        [HttpGet("TraLoi")]
        public async Task<IActionResult> TraLoi([FromQuery] string question)
        {
            if (string.IsNullOrEmpty(question))
                return BadRequest(new APIResponse { ResponseCode = 400, ErrorMessage = "Câu hỏi không được để trống." });

            var data = await _service.TraLoi(question);
            return Ok(data);
        }

        [HttpGet("TraLoiLienHe")]
        public async Task<IActionResult> TraLoiLienHe([FromQuery] string question)
        {
            if (string.IsNullOrEmpty(question))
                return BadRequest(new APIResponse { ResponseCode = 400, ErrorMessage = "Câu hỏi không được để trống." });

            var data = await _service.TraLoiLienHe(question);
            return Ok(data);
        }

        [HttpPost("Response")]
        public async Task<IActionResult> Response([FromBody] RequestOpenAIHinhAnh info)
        {
            if (info == null)
                return BadRequest(new APIResponse { ResponseCode = 400, ErrorMessage = "Dữ liệu không hợp lệ." });

            var data = await _service.Response(info);
            return Ok(data);
        }

        [HttpGet("PhanLoaiGopY")]
        public async Task<IActionResult> PhanLoaiGopY([FromQuery] string noiDung)
        {
            if (string.IsNullOrEmpty(noiDung))
                return BadRequest(new APIResponse { ResponseCode = 400, ErrorMessage = "Nội dung không được để trống." });

            var data = await _service.PhanLoaiGopY(noiDung);
            return Ok(data);
        }

        [HttpGet("SmartAI")]
        public async Task<IActionResult> SmartAI([FromQuery] string input)
        {
            if (string.IsNullOrEmpty(input))
                return BadRequest(new APIResponse { ResponseCode = 400, ErrorMessage = "Input không được để trống." });

            var data = await _service.TraLoiUpgrade(input);
            return Ok(data);
        }

        [HttpPost("ThemVaoGioHang")]
        public async Task<IActionResult> ThemVaoGioHang([FromBody] AddToCartRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.MaSanPham) || request.SoLuong <= 0)
                return BadRequest(new APIResponse { ResponseCode = 400, ErrorMessage = "Dữ liệu không hợp lệ." });

            string cartInfo = $"CART,{request.MaSanPham},{request.SoLuong}";
            var data = await _service.TraLoiUpgrade(cartInfo);
            return Ok(data);
        }
    }
}