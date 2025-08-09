using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Services;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class VoucherController : ControllerBase
    {
        private readonly IVoucherServices _voucherServices;

        public VoucherController(IVoucherServices voucherServices)
        {
            _voucherServices = voucherServices;
        }

        [HttpGet]
        public ActionResult<List<VoucherView>> GetAllVouchers()
        {
            try
            {
                var vouchers = _voucherServices.GetAllVouchers();
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("Validate")]
        public async Task<IActionResult> ValidateCoupon(string code, int cartId)
        {
            var response = await _voucherServices.ValidateCoupon(code, cartId);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [Authorize(Roles = "admin,staff")]
        [HttpPost]
        public async Task<ActionResult<VoucherView>> CreateVoucher([FromBody] VoucherCreate voucher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _voucherServices.CreateVoucher(voucher);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [Authorize(Roles = "admin,staff")]
        [HttpPut]
        public async Task<ActionResult<VoucherView>> EditVoucher([FromBody] VoucherEdit voucher)
        {
            if (!ModelState.IsValid || voucher.MaVoucher == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _voucherServices.EditVoucher(voucher);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [Authorize(Roles = "admin,staff")]
        [HttpDelete("{maVoucher}")]
        public async Task<ActionResult> DeleteVoucher(int maVoucher)
        {
            try
            {
                var result = await _voucherServices.DeleteVoucher(maVoucher);

                if (result)
                {
                    return NoContent(); 
                }
                else
                {
                    return NotFound("Voucher not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}"); 
            }
        }

        [HttpPut("Coupon/{couponId}")]
        public async Task<ActionResult> UpdateCoupon(int couponId, [FromBody] UpdateCouponRequest request)
        {
            if (!ModelState.IsValid || request.MaNguoiDung == null)
            {
                return BadRequest("Dữ liệu không hợp lệ hoặc thiếu mã người dùng.");
            }

            try
            {
                var result = await _voucherServices.UpdateCoupon(couponId, request.MaNguoiDung);
                if (result)
                {
                    return Ok(new { Message = "Lưu mã coupon thành công." });
                }
                else
                {
                    return NotFound("Không tìm thấy coupon hoặc coupon đã được sử dụng.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // Model cho request body
        public class UpdateCouponRequest
        {
            public string MaNguoiDung { get; set; }
        }

    }
}