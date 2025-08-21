using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UltraStrore.Models.CreateModels;
using UltraStrore.Repository;
using UltraStrore.Utils;

namespace UltraStrore.Services
{
    public class GoogleApisServices : IGoogleApisServices
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleApisSettings _settings;
        private readonly ISanPhamServices _sanPhamServices;
        private readonly ICartServices _cartServices;

        public GoogleApisServices(HttpClient httpClient, IOptions<GoogleApisSettings> settings, ISanPhamServices sanPhamServices = null, ICartServices cartServices = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _sanPhamServices = sanPhamServices;
            _cartServices = cartServices;
        }

        public async Task<ImageGenerationResponse> GenerateImageAsync(ImageGenerationRequest request)
        {
            try
            {
                var parts = new List<object> { new { text = request.TextPrompt } };
                foreach (var base64Image in request.ImageBase64)
                {
                    parts.Add(new
                    {
                        inline_data = new
                        {
                            mime_type = "image/jpeg",
                            data = base64Image
                        }
                    });
                }

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts }
                    },
                    generationConfig = new
                    {
                        responseModalities = new[] { "TEXT", "IMAGE" }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _settings.ApiKey);

                var service = "gemini-2.0-flash-preview-image-generation:generateContent";
                var fullUrl = $"{_settings.ApiUrl}/{service}";
                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Phản hồi API Gemini: " + responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var errorMessage = errorResponse?.error?.message?.ToString() ?? "Unknown error";
                    return new ImageGenerationResponse
                    {
                        GeneratedImageBase64 = null,
                        Message = $"Lỗi từ API Gemini: {errorMessage}"
                    };
                }

                var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (responseData?.candidates == null || responseData.candidates.Count == 0)
                {
                    return new ImageGenerationResponse
                    {
                        GeneratedImageBase64 = null,
                        Message = "Không tìm thấy candidates trong phản hồi API Gemini."
                    };
                }

                var candidate = responseData.candidates[0];
                if (candidate?.content?.parts == null || candidate.content.parts.Count == 0)
                {
                    return new ImageGenerationResponse
                    {
                        GeneratedImageBase64 = null,
                        Message = "Phản hồi API Gemini không chứa parts."
                    };
                }

                string generatedImageBase64 = null;
                foreach (var part in candidate.content.parts)
                {
                    if (part?.inlineData?.data != null && part.inlineData.mimeType.ToString().StartsWith("image/"))
                    {
                        generatedImageBase64 = part.inlineData.data.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(generatedImageBase64))
                {
                    return new ImageGenerationResponse
                    {
                        GeneratedImageBase64 = null,
                        Message = "Không tìm thấy dữ liệu hình ảnh (inlineData) trong phản hồi API Gemini. Phản hồi: " + responseContent
                    };
                }

                try
                {
                    Convert.FromBase64String(generatedImageBase64);
                }
                catch (FormatException)
                {
                    return new ImageGenerationResponse
                    {
                        GeneratedImageBase64 = null,
                        Message = "Chuỗi base64 không hợp lệ được trả về từ API Gemini. Phản hồi: " + responseContent
                    };
                }

                return new ImageGenerationResponse
                {
                    GeneratedImageBase64 = generatedImageBase64,
                    Message = "Hình ảnh đã được tạo thành công"
                };
            }
            catch (Exception ex)
            {
                return new ImageGenerationResponse
                {
                    GeneratedImageBase64 = null,
                    Message = $"Đã xảy ra ngoại lệ: {ex.Message}"
                };
            }
        }

        public async Task<TextGenerationResponse> GenerateTextAsync(TextGenerationRequest request)
        {
            try
            {
                var parts = new List<object> { new { text = request.TextPrompt } };

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _settings.ApiKey);

                var service = "gemini-2.5-flash:generateContent";
                var fullUrl = $"{_settings.ApiUrl}/{service}";
                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Phản hồi API Gemini: " + responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var errorMessage = errorResponse?.error?.message?.ToString() ?? "Unknown error";
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = $"Lỗi từ API Gemini: {errorMessage}"
                    };
                }

                var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (responseData?.candidates == null || responseData.candidates.Count == 0)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Không tìm thấy candidates trong phản hồi API Gemini."
                    };
                }

                var candidate = responseData.candidates[0];
                if (candidate?.content?.parts == null || candidate.content.parts.Count == 0)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Phản hồi API Gemini không chứa parts."
                    };
                }

                string generatedText = null;
                foreach (var part in candidate.content.parts)
                {
                    if (part?.text != null)
                    {
                        generatedText = part.text.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(generatedText))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Không tìm thấy văn bản (text) trong phản hồi API Gemini. Phản hồi: " + responseContent
                    };
                }

                return new TextGenerationResponse
                {
                    GeneratedText = generatedText,
                    Message = "Văn bản đã được tạo thành công"
                };
            }
            catch (Exception ex)
            {
                return new TextGenerationResponse
                {
                    GeneratedText = null,
                    Message = $"Đã xảy ra ngoại lệ: {ex.Message}"
                };
            }
        }

        public async Task<TextGenerationResponse> SearchProductsAsync(TextGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TextPrompt))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Vui lòng cung cấp nội dung tìm kiếm."
                    };
                }

                if (_sanPhamServices == null)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Dịch vụ sản phẩm không được khởi tạo."
                    };
                }

                var products = await _sanPhamServices.ListSanPham(null);
                if (products == null || !products.Any())
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Không tìm thấy sản phẩm nào trong hệ thống."
                    };
                }

                var productData = new StringBuilder();
                productData.AppendLine("Danh sách sản phẩm:\n");
                foreach (var product in products)
                {
                    productData.AppendLine($"Tên Sản Phẩm: {product.Name}");
                    productData.AppendLine($"Mã Sản Phẩm: {product.ID}");
                    productData.AppendLine($"Màu sắc: {string.Join(", ", product.MauSac)}");
                    productData.AppendLine($"Kích thước: {string.Join(", ", product.KichThuoc)}");
                    productData.AppendLine($"Thương hiệu: {product.ThuongHieu}");
                    productData.AppendLine($"Link: https://fashionhub.name.vn/product/{product.ID}\n");
                }

                var prompt = $@"Dựa trên danh sách sản phẩm sau: 
                    {productData}
                    Lọc ra các sản phẩm phù hợp với yêu cầu tìm kiếm: '{request.TextPrompt}'.";

                var parts = new List<object> { new { text = prompt } };
                var requestBody = new { contents = new[] { new { parts } } };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _settings.ApiKey);

                var service = "gemini-2.5-flash:generateContent";
                var fullUrl = $"{_settings.ApiUrl}/{service}";
                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var errorMessage = errorResponse?.error?.message?.ToString() ?? "Unknown error";
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = $"Lỗi từ API Gemini: {errorMessage}"
                    };
                }

                var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                if (responseData?.candidates == null || responseData.candidates.Count == 0)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Không tìm thấy candidates trong phản hồi API Gemini."
                    };
                }

                var candidate = responseData.candidates[0];
                if (candidate?.content?.parts == null || candidate.content.parts.Count == 0)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Phản hồi API Gemini không chứa parts."
                    };
                }

                string generatedText = null;
                foreach (var part in candidate.content.parts)
                {
                    if (part?.text != null)
                    {
                        generatedText = part.text.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(generatedText))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Không tìm thấy văn bản trong phản hồi API Gemini."
                    };
                }

                return new TextGenerationResponse
                {
                    GeneratedText = generatedText,
                    Message = "Tìm kiếm sản phẩm thành công"
                };
            }
            catch (Exception ex)
            {
                return new TextGenerationResponse
                {
                    GeneratedText = null,
                    Message = $"Đã xảy ra lỗi: {ex.Message}"
                };
            }
        }

        public async Task<TextGenerationResponse> AddToCartAsync(TextGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TextPrompt))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Vui lòng cung cấp thông tin sản phẩm để thêm vào giỏ hàng."
                    };
                }

                if (_sanPhamServices == null || _cartServices == null)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Dịch vụ sản phẩm hoặc giỏ hàng không được khởi tạo."
                    };
                }

                var match = Regex.Match(request.TextPrompt,
                    @"Thêm sản phẩm (\w+),\s*màu (\w+),\s*kích thước (\w+),\s*số lượng (\d+),\s*khách hàng (\w+)");
                if (!match.Success)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Thông tin sản phẩm không đầy đủ hoặc không đúng định dạng. Vui lòng cung cấp đúng định dạng: 'Thêm sản phẩm [mã], màu [mã màu], kích thước [kích thước], số lượng [số lượng], khách hàng [mã khách hàng]'."
                    };
                }

                var productId = match.Groups[1].Value.Trim();
                var colorHex = match.Groups[2].Value.Trim();
                var size = match.Groups[3].Value.Trim();
                if (!int.TryParse(match.Groups[4].Value, out var quantity) || quantity <= 0)
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Số lượng không hợp lệ. Vui lòng cung cấp số lượng lớn hơn 0."
                    };
                }
                var customerId = match.Groups[5].Value.Trim();

                if (string.IsNullOrWhiteSpace(productId) || string.IsNullOrWhiteSpace(colorHex) ||
                    string.IsNullOrWhiteSpace(size) || string.IsNullOrWhiteSpace(customerId))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Thông tin sản phẩm hoặc khách hàng không hợp lệ."
                    };
                }

                var productIdRegex = new Regex(@"^[A-Z]\d{5}$");
                var colorHexRegex = new Regex(@"^[0-9A-Fa-f]{6}$");
                var sizeRegex = new Regex(@"^(S|M|L|XL|XXL)$");
                var customerIdRegex = new Regex(@"^(KH\d{3}|ND\d{5})$");
                if (!productIdRegex.IsMatch(productId))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Mã sản phẩm không hợp lệ. Định dạng phải là A00001 (1 chữ cái + 5 số)."
                    };
                }
                if (!colorHexRegex.IsMatch(colorHex))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Mã màu không hợp lệ. Định dạng phải là mã hex 6 ký tự (e.g., 000000)."
                    };
                }
                if (!sizeRegex.IsMatch(size))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Kích thước không hợp lệ. Phải là S, M, L, XL hoặc XXL."
                    };
                }
                if (!customerIdRegex.IsMatch(customerId))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = "Mã khách hàng không hợp lệ. Định dạng phải là KH + 3 số (e.g., KH001) hoặc ND + 5 số (e.g., ND00001)."
                    };
                }

                var maSanPham = $"{productId}_{colorHex}_{size}";
                var productExists = await _sanPhamServices.SanPhamByID(productId);
                if (!productExists.Any(p => p.MaSanPham == maSanPham))
                {
                    return new TextGenerationResponse
                    {
                        GeneratedText = null,
                        Message = $"Sản phẩm {maSanPham} không tồn tại trong hệ thống."
                    };
                }

                var cartRequest = new ChiTietGioHangSanPhamCreate
                {
                    IDSanPham = productId,
                    MauSac = colorHex,
                    KichThuoc = size,
                    SoLuong = quantity,
                    IDNguoiDung = customerId
                };

                Console.WriteLine($"cartRequest: {JsonConvert.SerializeObject(cartRequest)}");

                var cartResponse = await _cartServices.ThemSanPham(cartRequest);
                Console.WriteLine($"cartResponse: {JsonConvert.SerializeObject(cartResponse)}");

                switch (cartResponse.ResponseCode)
                {
                    case 201:
                        return new TextGenerationResponse
                        {
                            GeneratedText = $"Đã thêm sản phẩm {maSanPham} (số lượng: {quantity}) vào giỏ hàng của khách hàng {customerId} vào lúc {DateTime.Now}.",
                            Message = "Thêm vào giỏ hàng thành công."
                        };
                    case 401:
                        return new TextGenerationResponse
                        {
                            GeneratedText = null,
                            Message = $"Người dùng {customerId} không tồn tại trong hệ thống."
                        };
                    case 500:
                        return new TextGenerationResponse
                        {
                            GeneratedText = null,
                            Message = cartResponse.Result?.ToString() ?? "Lỗi hệ thống khi thêm sản phẩm vào giỏ hàng."
                        };
                    default:
                        return new TextGenerationResponse
                        {
                            GeneratedText = null,
                            Message = cartResponse.Result?.ToString() ?? "Lỗi không xác định khi thêm sản phẩm vào giỏ hàng."
                        };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong AddToCartAsync: {ex.Message}\n{ex.StackTrace}");
                return new TextGenerationResponse
                {
                    GeneratedText = null,
                    Message = $"Đã xảy ra lỗi: {ex.Message}"
                };
            }
        }
    }
}