using UltraStrore.Utils;
using UltraStrore.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UltraStrore.Services
{
    public class GHNService : IGHNService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _shopId;
        private readonly ILogger<GHNService> _logger;

        public GHNService(HttpClient httpClient, IConfiguration configuration, ILogger<GHNService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // Use sandbox URL for testing; switch to production URL when ready
            _httpClient.BaseAddress = new Uri("https://dev-online-gateway.ghn.vn/shiip/public-api/");

            _apiKey = configuration["GHN:ApiKey"] ?? throw new ArgumentNullException("GHN:ApiKey is missing in configuration");
            _shopId = configuration["GHN:ShopId"] ?? throw new ArgumentNullException("GHN:ShopId is missing in configuration");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Province>> GetProvinces()
        {
            try
            {
                _logger.LogInformation("Fetching provinces from GHN API");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);

                var response = await _httpClient.GetAsync("master-data/province");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"GHN API returned status {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"GHN API error: {response.StatusCode} - {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"GHN API response: {content}");

                var result = JsonConvert.DeserializeObject<ProvinceResponse>(content);
                if (result == null || result.Data == null)
                {
                    _logger.LogWarning("No provinces found in GHN API response");
                    return new List<Province>();
                }

                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch provinces from GHN API");
                throw new Exception("Failed to fetch provinces", ex);
            }
        }

        public async Task<List<District>> GetDistricts(int provinceId)
        {
            try
            {
                _logger.LogInformation($"Fetching districts for province ID {provinceId} from GHN API");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);

                var response = await _httpClient.GetAsync($"master-data/district?province_id={provinceId}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"GHN API returned status {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"GHN API error: {response.StatusCode} - {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"GHN API response: {content}");

                var result = JsonConvert.DeserializeObject<DistrictResponse>(content);
                if (result == null || result.Data == null)
                {
                    _logger.LogWarning($"No districts found for province ID {provinceId} in GHN API response");
                    return new List<District>();
                }

                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch districts for province ID {provinceId} from GHN API");
                throw new Exception($"Failed to fetch districts for province ID {provinceId}", ex);
            }
        }

        public async Task<List<Ward>> GetWards(int districtId)
        {
            try
            {
                _logger.LogInformation($"Fetching wards for district ID {districtId} from GHN API");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);

                var response = await _httpClient.GetAsync($"master-data/ward?district_id={districtId}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"GHN API returned status {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"GHN API error: {response.StatusCode} - {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"GHN API response: {content}");

                var result = JsonConvert.DeserializeObject<WardResponse>(content);
                if (result == null || result.Data == null)
                {
                    _logger.LogWarning($"No wards found for district ID {districtId} in GHN API response");
                    return new List<Ward>();
                }

                return result.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch wards for district ID {districtId} from GHN API");
                throw new Exception($"Failed to fetch wards for district ID {districtId}", ex);
            }
        }

        public async Task<string> CreateShippingOrder(ShippingOrder order)
        {
            try
            {
                if (order == null)
                {
                    _logger.LogError("Shipping order is null");
                    throw new ArgumentNullException(nameof(order));
                }

                _logger.LogInformation("Creating shipping order with GHN API");
                var json = JsonConvert.SerializeObject(order);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);

                var response = await _httpClient.PostAsync("v2/shipping-order/create", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"GHN API returned status {response.StatusCode}: {errorContent}");
                    throw new HttpRequestException($"GHN API error: {response.StatusCode} - {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"GHN API response: {responseContent}");

                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                if (result?.data?.order_code == null)
                {
                    _logger.LogWarning("Failed to create shipping order: order_code not found in response");
                    throw new Exception("Failed to create shipping order: order_code not found in response");
                }

                return result.data.order_code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create shipping order with GHN API");
                throw new Exception("Failed to create shipping order", ex);
            }
        }
    }
}