using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiaoDienController : ControllerBase
    {
        private readonly IGiaoDienServices _services;

        public GiaoDienController(IGiaoDienServices services)
        {
            _services = services;
        }

        // GET: api/GiaoDien
        [HttpGet]
        public async Task<IActionResult> GetAllGiaoDien()
        {
            try
            {
                var list = await _services.GetAllGiaoDienAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/GiaoDien/{maGiaoDien}
        [HttpGet("{maGiaoDien}")]
        public async Task<IActionResult> GetGiaoDien(int maGiaoDien)
        {
            try
            {
                var giaoDien = await _services.GetGiaoDienAsync(maGiaoDien);
                return Ok(giaoDien);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/GiaoDien
        [HttpPost]
        public async Task<IActionResult> CreateGiaoDien([FromForm] GiaoDienCreate model)
        {
            try
            {
                model.TenGiaoDien = HttpContext.Request.Form["TenGiaoDien"];
                model.Logo = await ConvertToByteArray(HttpContext.Request.Form.Files["Logo"]);
                model.Slider1 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider1"]);
                model.Slider2 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider2"]);
                model.Slider3 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider3"]);
                model.Slider4 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider4"]);
                model.Avt = await ConvertToByteArray(HttpContext.Request.Form.Files["Avt"]);

                var createdGiaoDien = await _services.CreateGiaoDienAsync(model);
                return CreatedAtAction(nameof(GetGiaoDien), new { maGiaoDien = createdGiaoDien.MaGiaoDien }, createdGiaoDien);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/GiaoDien/{maGiaoDien}
        [HttpPut("{maGiaoDien}")]
        public async Task<IActionResult> UpdateGiaoDien(int maGiaoDien, [FromForm] GiaoDienEdit model)
        {
            if (model.MaGiaoDien == null || maGiaoDien != model.MaGiaoDien)
                return BadRequest("Mã giao diện không hợp lệ hoặc không khớp.");

            try
            {
                model.TenGiaoDien = HttpContext.Request.Form["TenGiaoDien"];
                model.Logo = HttpContext.Request.Form.Files["Logo"] != null
                    ? await ConvertToByteArray(HttpContext.Request.Form.Files["Logo"])
                    : null;
                model.Slider1 = HttpContext.Request.Form.Files["Slider1"] != null
                    ? await ConvertToByteArray(HttpContext.Request.Form.Files["Slider1"])
                    : null;
                model.Slider2 = HttpContext.Request.Form.Files["Slider2"] != null
                    ? await ConvertToByteArray(HttpContext.Request.Form.Files["Slider2"])
                    : null;
                model.Slider3 = HttpContext.Request.Form.Files["Slider3"] != null
                    ? await ConvertToByteArray(HttpContext.Request.Form.Files["Slider3"])
                    : null;
                model.Slider4 = HttpContext.Request.Form.Files["Slider4"] != null
                    ? await ConvertToByteArray(HttpContext.Request.Form.Files["Slider4"])
                    : null;
                model.Avt = HttpContext.Request.Form.Files["Avt"] != null
                    ? await ConvertToByteArray(HttpContext.Request.Form.Files["Avt"])
                    : null;

                var updatedGiaoDien = await _services.UpdateGiaoDienAsync(model);
                return Ok(updatedGiaoDien);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/GiaoDien/{maGiaoDien}
        [HttpDelete("{maGiaoDien}")]
        public async Task<IActionResult> DeleteGiaoDien(int maGiaoDien)
        {
            try
            {
                var result = await _services.DeleteGiaoDienAsync(maGiaoDien);
                if (!result)
                    return NotFound("Giao diện không tồn tại.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/GiaoDien/SetActive/{maGiaoDien}
        [HttpPut("SetActive/{maGiaoDien}")]
        public async Task<IActionResult> SetActiveGiaoDien(int maGiaoDien)
        {
            try
            {
                await _services.SetActiveGiaoDienAsync(maGiaoDien);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/GiaoDien/Search
        [HttpGet("Search")]
        public async Task<IActionResult> SearchGiaoDien(
            [FromQuery] string? tenGiaoDien,
            [FromQuery] int? maGiaoDien,
            [FromQuery] int? trangThai,
            [FromQuery] DateTime? ngayTao)
        {
            try
            {
                var result = await _services.SearchGiaoDienAsync(tenGiaoDien, maGiaoDien, trangThai, ngayTao);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Chuyển đổi IFormFile thành byte[]
        private async Task<byte[]> ConvertToByteArray(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedTypes = new[] { "image/png", "image/jpeg" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new Exception("Chỉ chấp nhận tệp PNG hoặc JPEG.");

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}