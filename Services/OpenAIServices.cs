using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UltraStrore.Data;
using UltraStrore.Helper;
using UltraStrore.Repository;
using UltraStrore.Utils;

namespace UltraStrore.Services
{
    public class OpenAIServices : IOpenAIServices
    {
        private readonly OpenAISettings _authSettings;
        private readonly ApplicationDbContext _context;
        private readonly ISanPhamServices _sanPhamServicesAddOn;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OpenAIServices(IOptions<OpenAISettings> authSettings, ApplicationDbContext context,
            ISanPhamServices sanPhamServicesAddOn, IHttpClientFactory httpClientFactory,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _authSettings = authSettings.Value;
            _context = context;
            _sanPhamServicesAddOn = sanPhamServicesAddOn;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<APIResponse> CallOpenAIApi(string prompt, string model = null)
        {
            APIResponse response = new APIResponse();
            try
            {
                var requestBody = new
                {
                    model = model ?? _authSettings.DefaultModel,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 1000
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                using (var client = _httpClientFactory.CreateClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authSettings.ApiKey);
                    var apiResponse = await client.PostAsync($"{_authSettings.ApiUrl}/chat/completions", content);

                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        response.ResponseCode = (int)apiResponse.StatusCode;
                        response.ErrorMessage = $"OpenAI API error: {apiResponse.ReasonPhrase}";
                        return response;
                    }

                    var responseString = await apiResponse.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string answer = responseObject?.choices[0]?.message?.content?.ToString() ?? "Xin lỗi, không thể xử lý yêu cầu.";
                    response.ResponseCode = 201;
                    response.Result = answer;
                }
            }
            catch (HttpRequestException ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = $"Network error: {ex.Message}";
            }
            catch (JsonException ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = $"JSON parsing error: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> TraLoi(string userInput)
        {
            return await CallOpenAIApi(userInput);
        }

        public async Task<APIResponse> TraLoiLienHe(string userInput)
        {
            string opening = "Bạn là nhân viên của một cửa hàng thời trang, hãy trả lời thắc mắc của khách hàng một cách lịch sự và chuyên nghiệp:\n";
            return await CallOpenAIApi($"{opening}{userInput}");
        }

        public async Task<APIResponse> Response(RequestOpenAIHinhAnh? info)
        {
            APIResponse response = new APIResponse();
            try
            {
                if (info == null || string.IsNullOrEmpty(info.CauHoi))
                {
                    response.ResponseCode = 400;
                    response.ErrorMessage = "Câu hỏi không được để trống.";
                    return response;
                }

                var messageContent = new List<object> { new { type = "text", text = info.CauHoi } };

                if (info.HinhAnh != null && info.HinhAnh.Count > 0)
                {
                    foreach (var imageBytes in info.HinhAnh)
                    {
                        string base64Image = Convert.ToBase64String(imageBytes);
                        messageContent.Add(new
                        {
                            type = "image_url",
                            image_url = new { url = $"data:image/png;base64,{base64Image}" }
                        });
                    }
                }

                var requestBody = new
                {
                    model = _authSettings.DefaultModel,
                    messages = new[]
                    {
                        new { role = "user", content = messageContent }
                    },
                    max_tokens = 1000
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                using (var client = _httpClientFactory.CreateClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authSettings.ApiKey);
                    var apiResponse = await client.PostAsync($"{_authSettings.ApiUrl}/chat/completions", content);

                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        response.ResponseCode = (int)apiResponse.StatusCode;
                        response.ErrorMessage = $"OpenAI API error: {apiResponse.ReasonPhrase}";
                        return response;
                    }

                    var responseString = await apiResponse.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string answer = responseObject?.choices[0]?.message?.content?.ToString() ?? "Xin lỗi, không thể xử lý yêu cầu.";
                    response.ResponseCode = 201;
                    response.Result = answer;
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> PhanLoaiGopY(string noiDung)
        {
            string prompt = $"Phân loại nội dung sau thành 'tích cực', 'tiêu cực' hoặc 'bình thường': {noiDung}\n" +
                            "Ví dụ:\n" +
                            "- 'Sản phẩm đẹp, chất lượng tốt' -> tích cực\n" +
                            "- 'Giao hàng chậm, sản phẩm lỗi' -> tiêu cực\n" +
                            "- 'Sản phẩm bình thường, giá hợp lý' -> bình thường";
            return await CallOpenAIApi(prompt);
        }

        public async Task<APIResponse> TraLoiUpgrade(string userInput)
        {
            APIResponse response = new APIResponse();
            try
            {
                string opening = "\nHãy dựa vào câu hỏi của khách hàng mà trả lời ngắn gọn theo yêu cầu dưới đây:\n" +
                                 "1. Nếu câu hỏi là về thông tin của một sản phẩm, trả lời định dạng: \"SP,Thông tin mà người dùng hỏi\". Ví dụ: \"SP,Quần có màu hồng của hãng Gucci\"\n" +
                                 "2. Nếu câu hỏi là về việc mua sản phẩm, kiểm tra xem khách hàng đã cung cấp đủ 2 dữ liệu: Mã Sản Phẩm, Số Lượng.\n" +
                                 "   - Nếu đủ, trả lời: \"CART,Mã Sản phẩm,Số Lượng\". Ví dụ: \"CART,A00001,10\"\n" +
                                 "   - Nếu thiếu, trả lời: \"CART!,null,null\" cho mỗi thông tin thiếu. Ví dụ: \"CART!,null,10\"";

                var apiResponse = await CallOpenAIApi($"{userInput}\n{opening}");
                if (apiResponse.ResponseCode != 201)
                    return apiResponse;

                string answer = apiResponse.Result;
                string finalAnswer;

                if (answer.StartsWith("SP"))
                    finalAnswer = await TimKiemSanPham(answer);
                else if (answer.StartsWith("CART"))
                {
                    var parts = answer.Split(',');
                    if (parts.Length != 3 || parts[0] != "CART")
                    {
                        response.ResponseCode = 400;
                        response.ErrorMessage = "Thông tin giỏ hàng không hợp lệ.";
                        return response;
                    }

                    string maSanPham = parts[1];
                    if (!int.TryParse(parts[2], out int soLuong))
                    {
                        response.ResponseCode = 400;
                        response.ErrorMessage = "Số lượng không hợp lệ.";
                        return response;
                    }

                    var request = new AddToCartRequest
                    {
                        MaSanPham = maSanPham,
                        SoLuong = soLuong
                    };

                    return await ThemVaoGioHang(request);
                }
                else
                    finalAnswer = "Yêu cầu không hợp lệ.";

                response.ResponseCode = 201;
                response.Result = finalAnswer;
                return response;
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }

        public async Task<APIResponse> ThemVaoGioHang(AddToCartRequest request)
        {
            APIResponse response = new APIResponse();
            try
            {
                if (request == null || string.IsNullOrEmpty(request.MaSanPham) || request.SoLuong <= 0)
                {
                    response.ResponseCode = 400;
                    response.ErrorMessage = "Dữ liệu giỏ hàng không hợp lệ.";
                    return response;
                }

                var sanPham = await _context.SanPhams.FirstOrDefaultAsync(s => s.MaSanPham == request.MaSanPham);
                if (sanPham == null)
                {
                    response.ResponseCode = 400;
                    response.ErrorMessage = "Mã sản phẩm không hợp lệ.";
                    return response;
                }
                int? gia = sanPham.Gia;

                int? thanhTien = gia * request.SoLuong;

                string userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "default_user";
                var gioHang = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaNguoiDung == userId);
                if (gioHang == null)
                {
                    gioHang = new GioHang
                    {
                        MaNguoiDung = userId
                    };
                    _context.GioHangs.Add(gioHang);
                    await _context.SaveChangesAsync();
                }

                var cartItem = new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaSanPham = request.MaSanPham,
                    SoLuong = request.SoLuong,
                    MaCombo = null,
                    Gia = gia,
                    ThanhTien = thanhTien
                };

                _context.ChiTietGioHangs.Add(cartItem);
                await _context.SaveChangesAsync();

                response.ResponseCode = 201;
                response.Result = $"Đã thêm sản phẩm {request.MaSanPham} (Số lượng: {request.SoLuong}, Tổng tiền: {thanhTien}) vào giỏ hàng.";
                return response;
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = $"Lỗi khi thêm vào giỏ hàng: {ex.Message}";
                return response;
            }
        }

        private async Task<string> TimKiemSanPham(string name)
        {
            string search = name.Split(',')[1]?.ToLower().Trim() ?? "Toàn bộ sản phẩm";
            var listSanPham = await _sanPhamServicesAddOn.ListSanPham(null);
            var data = listSanPham.ToList();

            if (!data.Any())
                return "Cửa hàng chúng tôi không bán sản phẩm này";

            var dataShow = new StringBuilder("Đây là danh sách sản phẩm mà khách hàng cần tìm: ");
            for (int i = 0; i < data.Count; i++)
            {
                dataShow.Append($"<br>{i + 1}. <strong>Tên Sản Phẩm: {data[i].Name}</strong>");
                dataShow.Append($" Thương hiệu: {data[i].ThuongHieu}");
                dataShow.Append($" <a href=\"{_configuration["BaseUrl"]}/product/{data[i].ID}\" style=\"color: #0000FF; text-decoration: underline;\" class=\"product-link\">Xem chi tiết sản phẩm</a>");
            }

            string prompt = $"Dựa trên {dataShow}, lọc ra những sản phẩm phù hợp với nội dung tìm kiếm là {search}, sau đó đóng vai nhân viên bán hàng. Trả về nội dung với các yêu cầu sau:\r\n" +
                           "1. Sử dụng thẻ <br> để ngắt dòng, không sử dụng \\n.\r\n" +
                           "2. Mỗi sản phẩm gắn một liên kết trong thẻ <a href=\"...\"> với văn bản \"Xem chi tiết sản phẩm\". Thẻ <a> có màu xanh (#0000FF) và hover thành xanh đậm (#000099).\r\n" +
                           "3. Định dạng: Tiêu đề sản phẩm in đậm (<strong>), mô tả, liên kết.\r\n" +
                           "4. Kết thúc bằng câu hỏi mời gọi khách hàng.\r\n" +
                           "<style>\r\n.product-link:hover {{ color: #000099; }}\r\n</style>";

            var apiResponse = await CallOpenAIApi(prompt);
            return apiResponse.ResponseCode == 201 ? apiResponse.Result : "Không thể lấy dữ liệu về sản phẩm";
        }
    }
}