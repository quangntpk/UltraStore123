using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

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
                    ? "http://localhost:8080"
                    : "http://localhost:8081";
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
    }
}