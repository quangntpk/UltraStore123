using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Services;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;

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

    }
}