using UltraStrore.Repository;
using UltraStrore.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UltraStrore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GHNController : ControllerBase
    {
        private readonly IGHNService _ghnService;
        private readonly ILogger<GHNController> _logger;

        public GHNController(IGHNService ghnService, ILogger<GHNController> logger)
        {
            _ghnService = ghnService ?? throw new ArgumentNullException(nameof(ghnService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("provinces")]
        public async Task<ActionResult<List<Province>>> GetProvinces()
        {
            try
            {
                _logger.LogInformation("Fetching provinces via API");
                var provinces = await _ghnService.GetProvinces();
                return Ok(provinces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch provinces");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to fetch provinces", details = ex.Message });
            }
        }

        [HttpGet("districts/{provinceId}")]
        public async Task<ActionResult<List<District>>> GetDistricts(int provinceId)
        {
            try
            {
                _logger.LogInformation($"Fetching districts for province ID {provinceId} via API");
                var districts = await _ghnService.GetDistricts(provinceId);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch districts for province ID {provinceId}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Failed to fetch districts for province ID {provinceId}", details = ex.Message });
            }
        }

        [HttpGet("wards/{districtId}")]
        public async Task<ActionResult<List<Ward>>> GetWards(int districtId)
        {
            try
            {
                _logger.LogInformation($"Fetching wards for district ID {districtId} via API");
                var wards = await _ghnService.GetWards(districtId);
                return Ok(wards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch wards for district ID {districtId}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Failed to fetch wards for district ID {districtId}", details = ex.Message });
            }
        }

        [HttpPost("shipping-order")]
        public async Task<ActionResult<string>> CreateShippingOrder([FromBody] ShippingOrder order)
        {
            try
            {
                if (order == null)
                {
                    _logger.LogWarning("Shipping order is null");
                    return BadRequest("Shipping order cannot be null");
                }

                _logger.LogInformation("Creating shipping order via API");
                var orderCode = await _ghnService.CreateShippingOrder(order);
                return Ok(orderCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create shipping order");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create shipping order", details = ex.Message });
            }
        }
    }
}