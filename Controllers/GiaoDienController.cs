using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using Microsoft.AspNetCore.SignalR;
using UltraStrore.Hubs;
using Microsoft.Extensions.Logging;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiaoDienController : ControllerBase
    {
        private readonly IGiaoDienServices _services;
        private readonly IHubContext<GiaoDienHub> _hubContext;
        private readonly ILogger<GiaoDienController> _logger;

        public GiaoDienController(
            IGiaoDienServices services,
            IHubContext<GiaoDienHub> hubContext,
            ILogger<GiaoDienController> logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGiaoDien()
        {
            try
            {
                _logger.LogInformation("Lấy danh sách giao diện.");
                var list = await _services.GetAllGiaoDienAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách giao diện.");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{maGiaoDien}")]
        public async Task<IActionResult> GetGiaoDien(int maGiaoDien)
        {
            try
            {
                _logger.LogInformation("Lấy thông tin giao diện với ID: {MaGiaoDien}", maGiaoDien);
                var giaoDien = await _services.GetGiaoDienAsync(maGiaoDien);
                return Ok(giaoDien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin giao diện với ID: {MaGiaoDien}", maGiaoDien);
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGiaoDien([FromForm] GiaoDienCreate model)
        {
            try
            {
                _logger.LogInformation("Tạo giao diện mới với tên: {TenGiaoDien}", model.TenGiaoDien);
                model.TenGiaoDien = HttpContext.Request.Form["TenGiaoDien"];
                model.Logo = await ConvertToByteArray(HttpContext.Request.Form.Files["Logo"]);
                model.Slider1 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider1"]);
                model.Slider2 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider2"]);
                model.Slider3 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider3"]);
                model.Slider4 = await ConvertToByteArray(HttpContext.Request.Form.Files["Slider4"]);
                model.Avt = await ConvertToByteArray(HttpContext.Request.Form.Files["Avt"]);

                var createdGiaoDien = await _services.CreateGiaoDienAsync(model);
                await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienAdded", createdGiaoDien);
                _logger.LogInformation("Tạo giao diện mới thành công với ID: {MaGiaoDien}", createdGiaoDien.MaGiaoDien);

                return CreatedAtAction(nameof(GetGiaoDien), new { maGiaoDien = createdGiaoDien.MaGiaoDien }, createdGiaoDien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo giao diện mới.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{maGiaoDien}")]
        public async Task<IActionResult> UpdateGiaoDien(int maGiaoDien, [FromForm] GiaoDienEdit model)
        {
            if (model.MaGiaoDien == null || maGiaoDien != model.MaGiaoDien)
            {
                _logger.LogWarning("Mã giao diện không hợp lệ hoặc không khớp: {MaGiaoDien}", maGiaoDien);
                return BadRequest("Mã giao diện không hợp lệ hoặc không khớp.");
            }

            try
            {
                _logger.LogInformation("Cập nhật giao diện với ID: {MaGiaoDien}", maGiaoDien);
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
                await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienUpdated", updatedGiaoDien);
                _logger.LogInformation("Cập nhật giao diện thành công với ID: {MaGiaoDien}", maGiaoDien);

                return Ok(updatedGiaoDien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giao diện với ID: {MaGiaoDien}", maGiaoDien);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{maGiaoDien}")]
        public async Task<IActionResult> DeleteGiaoDien(int maGiaoDien)
        {
            try
            {
                _logger.LogInformation("Xóa giao diện với ID: {MaGiaoDien}", maGiaoDien);
                var result = await _services.DeleteGiaoDienAsync(maGiaoDien);
                if (!result)
                {
                    _logger.LogWarning("Không tìm thấy giao diện để xóa với ID: {MaGiaoDien}", maGiaoDien);
                    return NotFound("Giao diện không tồn tại.");
                }

                await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienDeleted", maGiaoDien);
                _logger.LogInformation("Xóa giao diện thành công với ID: {MaGiaoDien}", maGiaoDien);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giao diện với ID: {MaGiaoDien}", maGiaoDien);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("SetActive/{maGiaoDien}")]
        public async Task<IActionResult> SetActiveGiaoDien(int maGiaoDien)
        {
            try
            {
                _logger.LogInformation("Đặt giao diện làm hoạt động với ID: {MaGiaoDien}", maGiaoDien);
                await _services.SetActiveGiaoDienAsync(maGiaoDien);
                await _hubContext.Clients.All.SendAsync("ReceiveGiaoDienSetActive", maGiaoDien);
                _logger.LogInformation("Đặt giao diện làm hoạt động thành công với ID: {MaGiaoDien}", maGiaoDien);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt giao diện làm hoạt động với ID: {MaGiaoDien}", maGiaoDien);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchGiaoDien(
            [FromQuery] string? tenGiaoDien,
            [FromQuery] int? maGiaoDien,
            [FromQuery] int? trangThai,
            [FromQuery] DateTime? ngayTao)
        {
            try
            {
                _logger.LogInformation("Tìm kiếm giao diện với tiêu chí: TenGiaoDien={TenGiaoDien}, MaGiaoDien={MaGiaoDien}, TrangThai={TrangThai}, NgayTao={NgayTao}",
                    tenGiaoDien, maGiaoDien, trangThai, ngayTao);
                var result = await _services.SearchGiaoDienAsync(tenGiaoDien, maGiaoDien, trangThai, ngayTao);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm giao diện.");
                return BadRequest(ex.Message);
            }
        }

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
