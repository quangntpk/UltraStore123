using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UltraStrore.Repository;
using UltraStrore.Utils;

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
            _httpClient.BaseAddress = new Uri("https://dev-online-gateway.ghn.vn/shiip/public-api/");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "UltraStrore-App/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _apiKey = configuration["GHN:ApiKey"] ?? throw new ArgumentNullException("Thiếu GHN:ApiKey trong cấu hình");
            _shopId = configuration["GHN:ShopId"] ?? throw new ArgumentNullException("Thiếu GHN:ShopId trong cấu hình");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Province>> GetProvinces()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);

            var response = await _httpClient.GetAsync("master-data/province");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProvinceResponse>(content)?.Data ?? new List<Province>();
        }

        public async Task<List<District>> GetDistricts(int provinceId)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);

            var response = await _httpClient.GetAsync($"master-data/district?province_id={provinceId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DistrictResponse>(content)?.Data ?? new List<District>();
        }

        public async Task<List<Ward>> GetWards(int districtId)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);

            var response = await _httpClient.GetAsync($"master-data/ward?district_id={districtId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WardResponse>(content)?.Data ?? new List<Ward>();
        }

        public async Task<string> CreateShippingOrder(ShippingOrder order)
        {
            try
            {
                if (order == null)
                {
                    _logger.LogError("Đơn hàng vận chuyển không được để trống");
                    throw new ArgumentNullException(nameof(order), "Đơn hàng vận chuyển không được để trống");
                }

                if (string.IsNullOrEmpty(order.to_name) || string.IsNullOrEmpty(order.to_phone) ||
                    string.IsNullOrEmpty(order.to_address) || string.IsNullOrEmpty(order.to_ward_name) ||
                    string.IsNullOrEmpty(order.to_district_name) || string.IsNullOrEmpty(order.to_province_name) ||
                    string.IsNullOrEmpty(order.to_ward_code) || order.to_district_id == null)
                {
                    _logger.LogError("Thiếu thông tin người nhận: to_name={to_name}, to_phone={to_phone}, to_address={to_address}, to_ward_name={to_ward_name}, to_district_name={to_district_name}, to_province_name={to_province_name}, to_ward_code={to_ward_code}, to_district_id={to_district_id}",
                        order.to_name, order.to_phone, order.to_address, order.to_ward_name, order.to_district_name, order.to_province_name, order.to_ward_code, order.to_district_id);
                    throw new ArgumentException("Thiếu thông tin người nhận (to_name, to_phone, to_address, to_ward_name, to_district_name, to_province_name, to_ward_code, to_district_id)");
                }

                if (string.IsNullOrEmpty(order.required_note) ||
                    (order.required_note != "CHOTHUHANG" && order.required_note != "CHOXEMHANGKHONGTHU" && order.required_note != "KHONGCHOXEMHANG"))
                {
                    _logger.LogError("Ghi chú bắt buộc không hợp lệ: required_note={required_note}", order.required_note);
                    throw new ArgumentException("Ghi chú bắt buộc phải là: CHOTHUHANG, CHOXEMHANGKHONGTHU hoặc KHONGCHOXEMHANG");
                }

                if (order.service_type_id == 2 && (order.weight <= 0 || order.length <= 0 || order.width <= 0 || order.height <= 0))
                {
                    _logger.LogError("Kích thước và khối lượng không hợp lệ cho hàng nhẹ: weight={weight}, length={length}, width={width}, height={height}",
                        order.weight, order.length, order.width, order.height);
                    throw new ArgumentException("Kích thước (length, width, height) và khối lượng (weight) phải lớn hơn 0 khi sử dụng dịch vụ hàng nhẹ");
                }

                if (order.service_type_id == 5)
                {
                    if (order.items == null || order.items.Count == 0)
                    {
                        _logger.LogError("Danh sách mặt hàng trống khi sử dụng dịch vụ hàng nặng (service_type_id=5)");
                        throw new ArgumentException("Danh sách mặt hàng (items) không được để trống khi sử dụng dịch vụ hàng nặng");
                    }

                    foreach (var item in order.items)
                    {
                        if (item.weight <= 0 || item.length <= 0 || item.width <= 0 || item.height <= 0)
                        {
                            _logger.LogError("Kích thước và khối lượng mặt hàng không hợp lệ: name={name}, weight={weight}, length={length}, width={width}, height={height}",
                                item.name, item.weight, item.length, item.width, item.height);
                            throw new ArgumentException("Kích thước (length, width, height) và khối lượng (weight) của mỗi mặt hàng phải lớn hơn 0 khi sử dụng dịch vụ hàng nặng");
                        }
                    }
                }

                _logger.LogInformation("Chuẩn bị gửi yêu cầu tạo đơn hàng tới API GHN");
                var json = JsonConvert.SerializeObject(order, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented 
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);

                _logger.LogDebug("Gửi yêu cầu POST tới API GHN: {RequestBody}", json);
                var response = await _httpClient.PostAsync("v2/shipping-order/create", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Phản hồi từ API GHN: StatusCode={StatusCode}, Response={ResponseContent}", response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API GHN trả về lỗi: StatusCode={StatusCode}, Response={ResponseContent}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"Lỗi API GHN: {response.StatusCode} - {responseContent}");
                }

                var result = JsonConvert.DeserializeObject<ShippingOrderResponse>(responseContent);
                if (result == null || result.data == null || string.IsNullOrEmpty(result.data.order_code))
                {
                    _logger.LogError("Phản hồi từ API GHN không chứa order_code: {ResponseContent}", responseContent);
                    throw new Exception("Không thể tạo đơn hàng vận chuyển: không tìm thấy order_code trong phản hồi");
                }

                _logger.LogInformation("Tạo đơn hàng thành công. Mã đơn hàng: {OrderCode}", result.data.order_code);
                return result.data.order_code;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Lỗi khi deserialize phản hồi từ API GHN: {Message}", ex.Message);
                throw new Exception("Lỗi xử lý phản hồi từ API GHN", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API GHN: {Message}", ex.Message);
                throw new Exception("Lỗi khi gọi API GHN", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tạo đơn hàng vận chuyển với API GHN: {Message}", ex.Message);
                throw new Exception("Không thể tạo đơn hàng vận chuyển", ex);
            }
        }

        public async Task<List<Shop>> GetShops(ShopRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);

            var response = await _httpClient.PostAsync("v2/shop/all", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShopResponse>(responseContent)?.data?.shops ?? new List<Shop>();
        }

        public async Task<LeadTimeResponseData> GetLeadTime(LeadTimeRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);

            var response = await _httpClient.PostAsync("v2/shipping-order/leadtime", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LeadTimeResponse>(responseContent);
            return result?.data ?? throw new Exception("Không tìm thấy dữ liệu thời gian dự kiến giao");
        }
        public async Task<ShippingOrderFee> GetShippingFee(ShippingFeeRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogError("Yêu cầu tính phí vận chuyển không được để trống");
                    throw new ArgumentNullException(nameof(request), "Yêu cầu tính phí vận chuyển không được để trống");
                }

                if (string.IsNullOrEmpty(request.to_ward_code) || request.to_district_id == 0)
                {
                    _logger.LogError("Thiếu thông tin người nhận: to_ward_code={to_ward_code}, to_district_id={to_district_id}",
                        request.to_ward_code, request.to_district_id);
                    throw new ArgumentException("Thiếu thông tin người nhận (to_ward_code, to_district_id)");
                }

                if (request.weight <= 0)
                {
                    _logger.LogError("Khối lượng không hợp lệ: weight={weight}", request.weight);
                    throw new ArgumentException("Khối lượng (weight) phải lớn hơn 0");
                }

                if (request.service_type_id == 5)
                {
                    if (request.items == null || request.items.Count == 0)
                    {
                        _logger.LogError("Danh sách mặt hàng trống khi sử dụng dịch vụ hàng nặng (service_type_id=5)");
                        throw new ArgumentException("Danh sách mặt hàng (items) không được để trống khi sử dụng dịch vụ hàng nặng");
                    }

                    foreach (var item in request.items)
                    {
                        if (item.weight <= 0 || item.length <= 0 || item.width <= 0 || item.height <= 0)
                        {
                            _logger.LogError("Kích thước và khối lượng mặt hàng không hợp lệ: name={name}, weight={weight}, length={length}, width={width}, height={height}",
                                item.name, item.weight, item.length, item.width, item.height);
                            throw new ArgumentException("Kích thước (length, width, height) và khối lượng (weight) của mỗi mặt hàng phải lớn hơn 0 khi sử dụng dịch vụ hàng nặng");
                        }
                    }
                }
                else if (request.service_type_id == 2 && (request.length <= 0 || request.width <= 0 || request.height <= 0))
                {
                    _logger.LogError("Kích thước không hợp lệ cho hàng nhẹ: length={length}, width={width}, height={height}",
                        request.length, request.width, request.height);
                    throw new ArgumentException("Kích thước (length, width, height) phải lớn hơn 0 khi sử dụng dịch vụ hàng nhẹ");
                }

                _logger.LogInformation("Chuẩn bị gửi yêu cầu tính phí vận chuyển tới API GHN");
                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Token", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId);

                _logger.LogDebug("Gửi yêu cầu POST tới API GHN: {RequestBody}", json);
                var response = await _httpClient.PostAsync("v2/shipping-order/fee", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Phản hồi từ API GHN: StatusCode={StatusCode}, Response={ResponseContent}", response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API GHN trả về lỗi: StatusCode={StatusCode}, Response={ResponseContent}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"Lỗi API GHN: {response.StatusCode} - {responseContent}");
                }

                var result = JsonConvert.DeserializeObject<ShippingFeeResponse>(responseContent);
                if (result == null || result.data == null)
                {
                    _logger.LogError("Phản hồi từ API GHN không chứa thông tin phí: {ResponseContent}", responseContent);
                    throw new Exception("Không thể tính phí vận chuyển: không tìm thấy thông tin phí trong phản hồi");
                }

                _logger.LogInformation("Tính phí vận chuyển thành công. Tổng phí: {TotalFee}", result.data.total);
                return result.data;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Lỗi khi deserialize phản hồi từ API GHN: {Message}", ex.Message);
                throw new Exception("Lỗi xử lý phản hồi từ API GHN", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi API GHN: {Message}", ex.Message);
                throw new Exception("Lỗi khi gọi API GHN", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tính phí vận chuyển với API GHN: {Message}", ex.Message);
                throw new Exception("Không thể tính phí vận chuyển", ex);
            }
        }

    }
}