using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Asn1.Cms;
using System.Security.Claims;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Utils;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XacThucController : ControllerBase
    {
        private readonly INguoiDungServices _nguoiDungServices;
        private readonly ITokenBlacklistService _blacklistService;
        private readonly IJwtTokenServices _jwtTokenServices;
        public XacThucController(INguoiDungServices nguoiDungServices, ITokenBlacklistService blacklistService,IJwtTokenServices jwtTokenServices)
        {
            _nguoiDungServices = nguoiDungServices;
            _blacklistService = blacklistService;
            _jwtTokenServices = jwtTokenServices;
        }

        [HttpPost("DangKy")]
        public async Task<IActionResult> DangKy([FromBody] DangKyView model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ketQua = await _nguoiDungServices.DangKy(model);

                return Ok(new
                {
                    message = "Đăng ký thành công",
                    user = ketQua
                });
            }
            catch (Exception ex) 
            {
                return BadRequest(new {message = ex.Message});  
            }
        }


        [HttpPost("VerifyOtpActivate")]
        public async Task<IActionResult> ActivateAccountlAsync([FromBody] XacMinhOtpView request)
        {
            try
            {
                var success = await _nguoiDungServices.ActivateAccountAsync(request.Email, request.Otp);
                if (!success)
                {
                    return BadRequest("Mã OTP không hợp lệ hoặc đã hết hạn");
                }

                return Ok(new { message = "Tài khoản của bạn đã được kích hoạt thành công!" });             
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("DangNhap")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapView model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (user, token) = await _nguoiDungServices.DangNhap(model);

                string redirectUrl = user.VaiTro == 1
                    ? "http://localhost:8081"
                    : "http://localhost:8080";
                return Ok(new
                {
                    message = "Đăng nhập thành công",
                    user,
                    token,
                    redirectUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "/api/XacThuc/google-callback")
        {
            // Lấy ClientId từ cấu hình
            var clientId = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Authentication:Google:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest(new { message = "ClientId của Google không được cấu hình" });
            }

            // Lấy IMemoryCache
            var memoryCache = HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

            // Tạo state ngẫu nhiên
            var state = Guid.NewGuid().ToString("N");

            memoryCache.Set($"OAuthState_{state}", state, TimeSpan.FromMinutes(5));

            // Xây dựng URL đăng nhập Google thủ công
            var redirectUri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{returnUrl}";
            var googleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={clientId}" +
                "&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                "&scope=openid%20email%20profile" +
                "&access_type=offline" +
                "&prompt=consent" +
                $"&state={state}";

            // Trả về URL để Swagger hiển thị
            return Ok(new { LoginUrl = googleAuthUrl });
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            // Lấy state từ query string
            var state = HttpContext.Request.Query["state"].ToString();

            // Lấy IMemoryCache
            var memoryCache = HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

            if (!memoryCache.TryGetValue($"OAuthState_{state}", out string storedState) || state != storedState)
            {
                return BadRequest(new { message = "OAuth state không hợp lệ" });
            }

            // Xóa state khỏi session sau khi sử dụng
            memoryCache.Remove($"OAuthState_{state}");

            // Lấy code từ query string
            var code = HttpContext.Request.Query["code"].ToString();
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "Không tìm thấy code từ Google" });
            }

            // Lấy ClientId và ClientSecret từ cấu hình
            var clientId = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Authentication:Google:ClientId"];
            var clientSecret = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Authentication:Google:ClientSecret"];
            var redirectUri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/XacThuc/google-callback";

            // Trao đổi code để lấy token
            using var httpClient = new HttpClient();
            var tokenRequest = new Dictionary<string, string>
    {
        { "code", code },
        { "client_id", clientId },
        { "client_secret", clientSecret },
        { "redirect_uri", redirectUri },
        { "grant_type", "authorization_code" }
    };

            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequest));
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                return BadRequest(new { message = "Không thể trao đổi code để lấy token", details = errorContent });
            }

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>();
            if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
            {
                return BadRequest(new { message = "Không thể lấy access token từ Google", details = tokenResponseContent });
            }

            // Sử dụng access token để lấy thông tin người dùng
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
            var userInfoResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            if (!userInfoResponse.IsSuccessStatusCode)
            {
                var errorContent = await userInfoResponse.Content.ReadAsStringAsync();
                return BadRequest(new { message = "Không thể lấy thông tin người dùng từ Google", details = errorContent });
            }

            var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<GoogleUserInfo>();
            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                return BadRequest(new { message = "Không thể lấy thông tin email từ Google" });
            }

            // Tiếp tục xử lý người dùng
            var existingUser = await _nguoiDungServices.GetNguoiDungByEmailAsync(userInfo.Email);

            if (existingUser == null)
            {
                var newUser = new NguoiDungCreate
                {
                    Email = userInfo.Email,
                    HoTen = userInfo.Name,
                    TaiKhoan = userInfo.Email,
                    VaiTro = 0,
                    TrangThai = 1,
                    NgayTao = DateTime.Now
                };

                var createdUser = await _nguoiDungServices.CreateNguoiDung(newUser);
                existingUser = await _nguoiDungServices.GetNguoiDungByEmailAsync(userInfo.Email);
            }

            // Tạo token
            var userView = new NguoiDungView
            {
                MaNguoiDung = existingUser.MaNguoiDung,
                TaiKhoan = existingUser.TaiKhoan,
                VaiTro = existingUser.VaiTro
            };

            var token = _jwtTokenServices.GenerateToken(userView);

            var userData = new
            {
                existingUser.MaNguoiDung,
                existingUser.HoTen,
                existingUser.Email,
                existingUser.VaiTro
            };

            string redirectUrl = existingUser.VaiTro == 1
               ? "http://localhost:8081"
               : "http://localhost:8080";

            var redirectWithData = $"{redirectUrl}?token={Uri.EscapeDataString(token)}&userId={existingUser.MaNguoiDung}&email={Uri.EscapeDataString(existingUser.Email)}&name={Uri.EscapeDataString(existingUser.HoTen)}&role={existingUser.VaiTro}";

            return Redirect(redirectWithData);
        }

        [HttpPost("DangXuat")]
        [Authorize] // Yêu cầu token hợp lệ
        public async Task<IActionResult> DangXuat()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token không hợp lệ." });
            }

            await _blacklistService.AddTokenToBlacklist(token, TimeSpan.FromMinutes(60));
            return Ok(new 
            {
                message = "Đăng xuất thành công",
                redirecTo = "http://localhost:8080/login?logout=true"
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var success = await _nguoiDungServices.GenerateAndSendOtpAsync(request.Email);
            if (!success)
            {
                return NotFound(new { message = "Email không tồn tại" });
            }

            return Ok(new { message = "Mã OTP đã được gửi đến email của bạn" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] XacMinhOtpView request)
        {
            var isValid = await _nguoiDungServices.VerifyOtpAsync(request.Email, request.Otp);
            if (!isValid)
            {
                return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn" });
            }

            return Ok(new { message = "Mã OTP hợp lệ" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] DatLaiMatKhauView request)
        {
            var success = await _nguoiDungServices.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword);
            if (!success)
            {
                return BadRequest(new { message = "Không thể đặt lại mật khẩu. Vui lòng kiểm tra OTP hoặc email" });
            }

            return Ok(new { message = "Mật khẩu đã được đặt lại thành công" });
        }
    }
}
