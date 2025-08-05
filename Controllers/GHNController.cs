using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltraStrore.Repository;
using UltraStrore.Utils;

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
                _logger.LogInformation("Đang lấy danh sách tỉnh/thành phố qua API");
                var provinces = await _ghnService.GetProvinces();
                return Ok(provinces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy danh sách tỉnh/thành phố");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Không thể lấy danh sách tỉnh/thành phố", details = ex.Message });
            }
        }

        [HttpGet("districts/{provinceId}")]
        public async Task<ActionResult<List<District>>> GetDistricts(int provinceId)
        {
            try
            {
                _logger.LogInformation($"Đang lấy danh sách quận/huyện cho tỉnh/thành phố ID {provinceId} qua API");
                var districts = await _ghnService.GetDistricts(provinceId);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Không thể lấy danh sách quận/huyện cho tỉnh/thành phố ID {provinceId}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Không thể lấy danh sách quận/huyện cho tỉnh/thành phố ID {provinceId}", details = ex.Message });
            }
        }

        [HttpGet("wards/{districtId}")]
        public async Task<ActionResult<List<Ward>>> GetWards(int districtId)
        {
            try
            {
                _logger.LogInformation($"Đang lấy danh sách phường/xã cho quận/huyện ID {districtId} qua API");
                var wards = await _ghnService.GetWards(districtId);
                return Ok(wards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Không thể lấy danh sách phường/xã cho quận/huyện ID {districtId}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Không thể lấy danh sách phường/xã cho quận/huyện ID {districtId}", details = ex.Message });
            }
        }
        [HttpPost("shipping-order")]
        public async Task<ActionResult<string>> CreateShippingOrder([FromBody] ShippingOrder order)
        {
            try
            {
                if (order == null)
                {
                    _logger.LogWarning("Đơn hàng vận chuyển không được để trống");
                    return BadRequest(new { message = "Đơn hàng vận chuyển không được để trống" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Dữ liệu đơn hàng vận chuyển không hợp lệ: {Errors}", JsonConvert.SerializeObject(errors));
                    return BadRequest(new { message = "Dữ liệu đơn hàng không hợp lệ", errors });
                }

                _logger.LogInformation("Đang tạo đơn hàng vận chuyển qua API");
                var orderCode = await _ghnService.CreateShippingOrder(order);
                return Ok(orderCode);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu đầu vào không hợp lệ: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API GHN: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Lỗi khi gọi API GHN", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tạo đơn hàng vận chuyển: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Không thể tạo đơn hàng vận chuyển", details = ex.Message });
            }
        }

        [HttpPost("shops")]
        public async Task<ActionResult<List<Shop>>> GetShops([FromBody] ShopRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Yêu cầu lấy danh sách cửa hàng không được để trống");
                    return BadRequest("Yêu cầu lấy danh sách cửa hàng không được để trống");
                }

                _logger.LogInformation($"Đang lấy danh sách cửa hàng qua API với offset={request.offset}, limit={request.limit}, client_phone={request.client_phone}");
                var shops = await _ghnService.GetShops(request);
                return Ok(shops);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy danh sách cửa hàng");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Không thể lấy danh sách cửa hàng", details = ex.Message });
            }
        }

        [HttpPost("leadtime")]
        public async Task<ActionResult<LeadTimeResponseData>> GetLeadTime([FromBody] LeadTimeRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Yêu cầu tính thời gian dự kiến giao không được để trống");
                    return BadRequest("Yêu cầu tính thời gian dự kiến giao không được để trống");
                }

                _logger.LogInformation($"Đang tính thời gian dự kiến giao qua API với from_district_id={request.from_district_id}, to_district_id={request.to_district_id}, service_id={request.service_id}");
                var leadTime = await _ghnService.GetLeadTime(request);
                return Ok(leadTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tính thời gian dự kiến giao");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Không thể tính thời gian dự kiến giao", details = ex.Message });
            }
        }

        [HttpPost("shipping-fee")]
        public async Task<ActionResult<ShippingOrderFee>> GetShippingFee([FromBody] ShippingFeeRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Yêu cầu tính phí vận chuyển không được để trống");
                    return BadRequest(new { message = "Yêu cầu tính phí vận chuyển không được để trống" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Dữ liệu yêu cầu tính phí vận chuyển không hợp lệ: {Errors}", JsonConvert.SerializeObject(errors));
                    return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ", errors });
                }

                _logger.LogInformation("Đang tính phí vận chuyển qua API");
                var fee = await _ghnService.GetShippingFee(request);
                return Ok(fee);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dữ liệu đầu vào không hợp lệ: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API GHN: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Lỗi khi gọi API GHN", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tính phí vận chuyển: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Không thể tính phí vận chuyển", details = ex.Message });
            }
        }

    }
}