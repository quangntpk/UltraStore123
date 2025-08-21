using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UltraStrore.Hubs;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    //[Authorize(Roles = "admin,staff")]
    [Route("api/[controller]")]
    [ApiController]
    public class LienHeController : ControllerBase
    {
        private readonly ILienHeServices _services;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LienHeController> _logger;
        private readonly IHubContext<LienHeHub> _hubContext;

        public LienHeController(
            ILienHeServices services,
            IConfiguration configuration,
            EmailService emailService,
            ILogger<LienHeController> logger,
            IHubContext<LienHeHub> hubContext)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _httpClient = new HttpClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm)
        {
            _logger.LogInformation("Lấy danh sách liên hệ với từ khóa: {SearchTerm}", searchTerm);
            var list = await _services.GetLienHeList(searchTerm);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Lấy thông tin liên hệ với ID: {Id}", id);
                var lienHe = await _services.GetLienHeById(id);
                return Ok(lienHe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin liên hệ với ID: {Id}", id);
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LienHeCreateRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(model.ReCaptchaToken))
            {
                _logger.LogWarning("Yêu cầu tạo liên hệ không có token reCAPTCHA.");
                return BadRequest("Chưa cung cấp token reCAPTCHA.");
            }

            bool isCaptchaValid = await VerifyRecaptcha(model.ReCaptchaToken);
            if (!isCaptchaValid)
            {
                _logger.LogWarning("Xác minh reCAPTCHA thất bại.");
                return BadRequest("Xác minh reCAPTCHA thất bại.");
            }

            var lienHeCreate = new LienHeCreate
            {
                HoTen = model.HoTen,
                Sdt = model.Sdt,
                NoiDung = model.NoiDung,
                Email = model.Email,
                TrangThai = int.TryParse(model.TrangThai, out int trangThai) ? trangThai : 0
            };

            try
            {
                var newLienHe = await _services.CreateLienHe(lienHeCreate);
                _logger.LogInformation("Tạo liên hệ mới thành công với ID: {Id}", newLienHe.MaLienHe);

                await _hubContext.Clients.All.SendAsync("ReceiveLienHeAdded", newLienHe);

                try
                {
                    string emailBody = $@"
                    <html>
                    <head>
                      <meta charset='utf-8' />
                      <title>Thông tin liên hệ mới</title>
                      <style>
                        body {{ margin: 0; padding: 0; background-color: #f4f4f4; font-family: 'Arial', sans-serif; color: #333; }}
                        .email-container {{ width: 100%; max-width: 650px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #9b87f5 0%, #6b5be3 100%); color: #ffffff; text-align: center; padding: 25px 0; }}
                        .header img {{ max-width: 150px; margin-bottom: 10px; }}
                        .header h1 {{ font-size: 28px; margin: 0; font-weight: 600; letter-spacing: 1px; }}
                        .content {{ padding: 30px; background-color: #f9f9f9; }}
                        .content h2 {{ font-size: 22px; color: #9b87f5; margin-bottom: 20px; font-weight: 500; border-bottom: 2px solid #9b87f5; padding-bottom: 8px; }}
                        .content p {{ margin: 12px 0; font-size: 16px; line-height: 1.6; }}
                        .content .label {{ font-weight: 600; color: #555; }}
                        .content .value {{ color: #333; }}
                        .footer {{ background-color: #9b87f5; color: #ffffff; text-align: center; padding: 15px; font-size: 14px; }}
                        .footer p {{ margin: 5px 0; }}
                        .footer a {{ color: #ffffff; text-decoration: none; font-weight: 500; }}
                        .footer a:hover {{ text-decoration: underline; }}
                      </style>
                    </head>
                    <body>
                      <div class='email-container'>
                        <div class='header'>
                          <img src='https://fashionhub.com.br/wp-content/uploads/2021/10/Fashion-Hub_logo-preta.png' alt='Logo FashionHub' />
                          <h1>Thông tin liên hệ mới</h1>
                        </div>
                        <div class='content'>
                          <h2>Chi tiết liên hệ</h2>
                          <p><span class='label'>Họ và tên:</span> <span class='value'>{newLienHe.HoTen ?? "N/A"}</span></p>
                          <p><span class='label'>Email:</span> <span class='value'>{newLienHe.Email ?? "N/A"}</span></p>
                          <p><span class='label'>Số điện thoại:</span> <span class='value'>{(string.IsNullOrWhiteSpace(newLienHe.Sdt) ? "N/A" : newLienHe.Sdt)}</span></p>
                          <p><span class='label'>Nội dung:</span><br/><span class='value'>{newLienHe.NoiDung ?? "N/A"}</span></p>
                          <p><span class='label'>Ngày gửi:</span> <span class='value'>{newLienHe.NgayTao?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A"}</span></p>
                        </div>
                        <div class='footer'>
                          <p>FashionHub © {DateTime.Now.Year}. Mọi quyền được bảo lưu.</p>
                          <p><a href='https://fashionhub.name.vn'>Truy cập trang web của chúng tôi</a></p>
                        </div>
                      </div>
                    </body>
                    </html>";
                    await _emailService.SendEmailAsync(_configuration["Smtp:Username"], "FashionHub - Thông tin liên hệ mới", emailBody);
                    _logger.LogInformation("Gửi email thông báo liên hệ mới thành công.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Lỗi khi gửi email thông báo liên hệ mới.");
                }

                return CreatedAtAction(nameof(GetById), new { id = newLienHe.MaLienHe }, newLienHe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo liên hệ mới.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] LienHeCreate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newLienHe = await _services.AddLienHe(model);
                _logger.LogInformation("Thêm liên hệ mới thành công với ID: {Id}", newLienHe.MaLienHe);
                await _hubContext.Clients.All.SendAsync("ReceiveLienHeAdded", newLienHe);
                return CreatedAtAction(nameof(GetById), new { id = newLienHe.MaLienHe }, newLienHe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm liên hệ mới.");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SupportEmail")]
        public async Task<IActionResult> SendSupportEmail([FromBody] SupportEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                string emailBody = $@"
                <html>
                <head>
                  <meta charset='utf-8' />
                  <title>Hỗ trợ từ FashionHub</title>
                  <style>
                    body {{ margin: 0; padding: 0; background-color: #f4f4f4; font-family: 'Arial', sans-serif; color: #333; }}
                    .email-container {{ width: 100%; max-width: 650px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); overflow: hidden; }}
                    .header {{ background: linear-gradient(135deg, #9b87f5 0%, #6b5be3 100%); color: #ffffff; text-align: center; padding: 25px 0; }}
                    .header img {{ max-width: 150px; margin-bottom: 10px; }}
                    .header h1 {{ font-size: 28px; margin: 0; font-weight: 600; letter-spacing: 1px; }}
                    .content {{ padding: 30px; background-color: #f9f9f9; }}
                    .content p {{ margin: 12px 0; font-size: 16px; line-height: 1.6; color: #333; }}
                    .footer {{ background-color: #9b87f5; color: #ffffff; text-align: center; padding: 15px; font-size: 14px; }}
                    .footer p {{ margin: 5px 0; }}
                    .footer a {{ color: #ffffff; text-decoration: none; font-weight: 500; }}
                    .footer a:hover {{ text-decoration: underline; }}
                  </style>
                </head>
                <body>
                  <div class='email-container'>
                    <div class='header'>
                      <img src='https://fashionhub.com.br/wp-content/uploads/2021/10/Fashion-Hub_logo-preta.png' alt='Logo FashionHub' />
                      <h1>Hỗ trợ từ FashionHub</h1>
                    </div>
                    <div class='content'>
                      <p>{request.Message}</p>
                    </div>
                    <div class='footer'>
                      <p>FashionHub © {DateTime.Now.Year}. Mọi quyền được bảo lưu.</p>
                      <p><a href='https://fashionhub.name.vn'>Truy cập trang web của chúng tôi</a></p>
                    </div>
                  </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(request.ToEmail, "FashionHub - Phản hồi hỗ trợ", emailBody);
                _logger.LogInformation("Gửi email hỗ trợ thành công tới: {ToEmail}", request.ToEmail);
                return Ok("Email hỗ trợ đã được gửi thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email hỗ trợ tới: {ToEmail}", request.ToEmail);
                return StatusCode(500, $"Lỗi khi gửi email hỗ trợ: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LienHeEdit model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.MaLienHe)
            {
                _logger.LogWarning("Mã liên hệ không khớp: {Id} != {MaLienHe}", id, model.MaLienHe);
                return BadRequest("Mã liên hệ không khớp.");
            }

            try
            {
                var updatedLienHe = await _services.UpdateLienHe(model);
                _logger.LogInformation("Cập nhật liên hệ thành công với ID: {Id}", id);
                await _hubContext.Clients.All.SendAsync("ReceiveLienHeUpdated", updatedLienHe);
                return Ok(updatedLienHe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật liên hệ với ID: {Id}", id);
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _services.DeleteLienHe(id);
                if (!result)
                {
                    _logger.LogWarning("Không tìm thấy liên hệ để xóa với ID: {Id}", id);
                    return NotFound("Liên hệ không tồn tại.");
                }
                _logger.LogInformation("Xóa liên hệ thành công với ID: {Id}", id);
                await _hubContext.Clients.All.SendAsync("ReceiveLienHeDeleted", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa liên hệ với ID: {Id}", id);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteMultiple")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {
            try
            {
                var result = await _services.DeleteMultipleLienHe(ids);
                if (!result)
                {
                    _logger.LogWarning("Không tìm thấy liên hệ để xóa với danh sách ID: {Ids}", string.Join(",", ids));
                    return NotFound("Không tìm thấy liên hệ để xóa.");
                }
                _logger.LogInformation("Xóa nhiều liên hệ thành công với danh sách ID: {Ids}", string.Join(",", ids));
                await _hubContext.Clients.All.SendAsync("ReceiveLienHeDeletedMultiple", ids);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhiều liên hệ.");
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        private async Task<bool> VerifyRecaptcha(string token)
        {
            var secretKey = _configuration["ReCaptcha:SecretKey"];
            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                null
            );

            if (response.IsSuccessStatusCode)
            {
                var recaptchaResponse = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
                return recaptchaResponse?.success == true;
            }
            return false;
        }
    }

    public class RecaptchaResponse
    {
        public bool success { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
        public List<string> error_codes { get; set; }
    }

    public class SupportEmailRequest
    {
        public string ToEmail { get; set; }
        public string Message { get; set; }
    }
}