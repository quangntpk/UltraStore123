using Microsoft.AspNetCore.Mvc;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Controllers
{
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
        public IActionResult GetOrderStatusStatistics()
        {
            var result = _thongKeServices.GetOrderStatusStatistics();
            return Ok(result);
        }
    }
}