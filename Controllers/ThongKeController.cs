using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
    [Authorize(Roles = "admin,staff")]
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly IThongKeServices _thongKeServices;

        public ThongKeController(IThongKeServices thongKeServices)
        {
            _thongKeServices = thongKeServices;
        }

        [HttpGet("Daily")]
        public IActionResult GetDailyStatistics(int year, int month, int day)
        {
            var result = _thongKeServices.GetDailyStatistics(year, month, day);
            return Ok(result);
        }

        [HttpGet("Monthly")]
        public IActionResult GetMonthlyStatistics(int year, int month)
        {
            var result = _thongKeServices.GetMonthlyStatistics(year, month);
            return Ok(result);
        }

        [HttpGet("Yearly")]
        public IActionResult GetYearlyStatistics(int year)
        {
            var result = _thongKeServices.GetYearlyStatistics(year);
            return Ok(result);
        }

        [HttpGet("OrderStatus")]
        public IActionResult GetOrderStatusStatistics(int? year = null, int? month = null, int? day = null)
        {
            var result = _thongKeServices.GetOrderStatusStatistics(year, month, day);
            return Ok(result);
        }

        [HttpGet("TopProducts")]
        public async Task<IActionResult> GetTopProductsStatistics(int year, int? month = null, int? day = null)
        {
            try
            {
                var result = await _thongKeServices.GetTopProductsStatistics(year, month, day);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching top products statistics.",
                    error = ex.Message
                });
            }
        }
    }
}