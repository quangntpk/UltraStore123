using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateProfileController : ControllerBase
    {
        private readonly INguoiDungServices _userService;

        public UpdateProfileController(INguoiDungServices userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var claims = User.FindAll(ClaimTypes.NameIdentifier).ToList();
            var maNguoiDung = claims.FirstOrDefault(c => c.Value.StartsWith("ND") || c.Value.StartsWith("KH"))?.Value;

            if (string.IsNullOrEmpty(maNguoiDung))
                return Unauthorized("Không tìm thấy thông tin người dùng.");

            var user = await _userService.GetNguoiDungById(maNguoiDung);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // Chuyển đổi HinhAnh từ byte[] sang Base64 để trả về
            var userDto = new
            {
                maNguoiDung = user.MaNguoiDung,
                hoTen = user.HoTen,
                email = user.Email,
                sdt = user.Sdt,
                diaChi = user.DiaChi,
                cccd = user.Cccd,
                ngaySinh = user.NgaySinh,
                hinhAnh = user.HinhAnh != null ? Convert.ToBase64String(user.HinhAnh) : null
            };

            return Ok(userDto);
        }


        [HttpPut("update-profile/{maNguoiDung}")]
        public async Task<IActionResult> UpdateProfile(string maNguoiDung, [FromForm] UpdateProfileView model)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"currentUserId: {currentUserId}, maNguoiDung: {maNguoiDung}");

                if (string.IsNullOrEmpty(model.HoTen))
                    return BadRequest("Họ tên không được để trống");

                if (!string.IsNullOrEmpty(model.Email) && !IsValidEmail(model.Email))
                    return BadRequest("Email không hợp lệ");

                if (!string.IsNullOrEmpty(model.Sdt) && !IsValidPhoneNumber(model.Sdt))
                    return BadRequest("Số điện thoại không hợp lệ");

                var result = await _userService.UpdateUserProfileAsync(maNguoiDung, model);

                if (result)
                    return Ok(new { message = "Cập nhật thông tin thành công" });
                else
                    return NotFound("Không tìm thấy người dùng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        [HttpPut("update-password/{maNguoiDung}")]
        public async Task<IActionResult> UpdatePassword(string maNguoiDung, [FromForm] UpdateProfileView model)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"currentUserId: {currentUserId}, maNguoiDung: {maNguoiDung}");

                if (string.IsNullOrEmpty(model.MatKhauCu))
                    return BadRequest("Mật khẩu không được để trống");

                var result = await _userService.UpdateUserPassword(maNguoiDung, model);

                if (result)
                    return Ok(new { message = "Cập nhật mật khẩu thành công" });
                else
                    return NotFound("Không tìm thấy người dùng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^[0-9]{10}$");
        }

    }
}
