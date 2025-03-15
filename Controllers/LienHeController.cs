using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LienHeController : ControllerBase
    {
        private readonly ILienHeServices _services;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LienHeController> _logger;

        public LienHeController(
            ILienHeServices services,
            IConfiguration configuration,
            EmailService emailService,
            ILogger<LienHeController> logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _httpClient = new HttpClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                _logger.LogError(ex, "Lỗi khi lấy liên hệ với ID: {Id}", id);
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
                MaLienHe = model.MaLienHe,
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

                try
                {
                    string emailBody = $@"
                    <html>
                    <head>
                      <meta charset='utf-8' />
                      <title>Thông tin liên hệ mới</title>
                      <style>
                        body {{ margin: 0; padding: 0; background-color: #f2f2f2; font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; }}
                        .email-container {{ width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; }}
                        .header {{ background-color: #007BFF; color: #ffffff; text-align: center; padding: 20px; }}
                        .header h1 {{ font-size: 24px; margin: 0; }}
                        .content {{ padding: 20px; color: #333333; line-height: 1.5; }}
                        .content h2 {{ font-size: 20px; color: #007BFF; }}
                        .content p {{ margin: 15px 0; font-size: 16px; }}
                        .footer {{ background-color: #f4f4f4; text-align: center; padding: 15px; font-size: 12px; color: #777777; }}
                      </style>
                    </head>
                    <body>
                      <div class='email-container'>
                        <div class='header'>
                          <h1>Thông tin liên hệ mới</h1>
                        </div>
                        <div class='content'>
                          <h2>Chi tiết liên hệ</h2>
                          <p><strong>Họ và tên:</strong> {newLienHe.HoTen ?? "N/A"}</p>
                          <p><strong>Email:</strong> {newLienHe.Email ?? "N/A"}</p>
                          <p><strong>Số điện thoại:</strong> {(string.IsNullOrWhiteSpace(newLienHe.Sdt) ? "N/A" : newLienHe.Sdt)}</p>
                          <p><strong>Nội dung:</strong><br/>{newLienHe.NoiDung ?? "N/A"}</p>
                        </div>
                        <div class='footer'>
                          <p>UltraStore © {DateTime.Now.Year}. Mọi quyền được bảo lưu.</p>
                          <p>Hệ thống bán hàng chuyên nghiệp</p>
                        </div>
                      </div>
                    </body>
                    </html>";
                    await _emailService.SendEmailAsync(_configuration["Smtp:Username"], "UltraStore", emailBody);

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
                  <title>Hỗ trợ từ UltraStore</title>
                  <style>
                    body {{ margin: 0; padding: 0; background-color: #f2f2f2; font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; }}
                    .email-container {{ width: 100%; max-width: 600px; margin: 20px auto; background-color: #ffffff; border: 1px solid #dddddd; border-radius: 8px; overflow: hidden; }}
                    .header {{ background-color: #007BFF; color: #ffffff; text-align: center; padding: 20px; }}
                    .header h1 {{ font-size: 24px; margin: 0; }}
                    .content {{ padding: 20px; color: #333333; line-height: 1.5; }}
                    .content p {{ margin: 15px 0; font-size: 16px; }}
                    .footer {{ background-color: #f4f4f4; text-align: center; padding: 15px; font-size: 12px; color: #777777; }}
                  </style>
                </head>
                <body>
                  <div class='email-container'>
                    <div class='header'>
                      <h1>Hỗ trợ từ UltraStore</h1>
                    </div>
                    <div class='content'>
                      <p>{request.Message}</p>
                    </div>
                    <div class='footer'>
                      <p>UltraStore © {DateTime.Now.Year}. Mọi quyền được bảo lưu.</p>
                      <p>Hệ thống bán hàng chuyên nghiệp</p>
                    </div>
                  </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(request.ToEmail, "Hỗ trợ từ UltraStore", emailBody);
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
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa liên hệ với ID: {Id}", id);
                return StatusCode(500, ex.Message);
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
}