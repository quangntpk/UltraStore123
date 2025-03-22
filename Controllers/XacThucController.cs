using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Models.EditModels;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XacThucController : ControllerBase
    {
        private readonly INguoiDungServices _nguoiDungServices;
        private readonly ITokenBlacklistService _blacklistService;
        public XacThucController(INguoiDungServices nguoiDungServices, ITokenBlacklistService blacklistService)
        {
            _nguoiDungServices = nguoiDungServices;
            _blacklistService = blacklistService;
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

                string redirectUrl = user.VaiTro == 0
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
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpPost("DangNhapAdmin")]
        public async Task<IActionResult> DangNhapAdmin([FromBody] LoginAdmin model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (user, token) = await _nguoiDungServices.DangNhapAdmin(model);

                return Ok(new
                {
                    message = "Đăng nhập Admin thành công",
                    user,
                    token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = "http://localhost:5261/api/XacThuc/google-callback" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return Redirect("http://localhost:3000/login?error=auth_failed");
            }

            var claims = authenticateResult.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var isAdmin = await _nguoiDungServices.IsAdminAsync(email);
            if (!isAdmin)
            {
                return Redirect("http://localhost:3000/login?error=unauthorized");
            }

            var (user, token) = await _nguoiDungServices.DangNhapGoogleAdmin(email);

            return Redirect($"http://localhost:3000/login?token={token}&user={user.MaNguoiDung}");
        }

        // PUT: api/XacThuc/chitiet/{id}
        [HttpPut("chitiet/{id}")]
        public async Task<IActionResult> UpdateChiTietUser(string id, [FromForm] ChiTietUser model)
        {
            // Kiểm tra xem id có khớp với MaNguoiDung trong model không
            if (id != model.MaNguoiDung)
            {
                return BadRequest(new { message = "Mã người dùng không khớp" });
            }

            try
            {
                // Gọi service để cập nhật thông tin chi tiết người dùng
                var updatedUser = await _nguoiDungServices.UpdateChiTietUser(model);
                return Ok(new
                {
                    message = "Cập nhật thông tin thành công",
                    user = updatedUser
                });
            }
            catch (Exception ex)
            {
                // Trả về NotFound nếu người dùng không tồn tại hoặc có lỗi khác
                return NotFound(new { message = ex.Message });
            }
        }
    }
}