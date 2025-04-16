using Newtonsoft.Json;
using UltraStrore.Helper;
using System.Text;
using UltraStrore.Repository;
using UltraStrore.Utils;
using UltraStrore.Models.CreateModels;
using Azure;

namespace UltraStrore.Services
{
    public class GeminiServices : IGeminiServices
    {
        private readonly GeminiSettings _authSettings;
        public GeminiServices(GeminiSettings authSettings)
        {
            _authSettings = authSettings;
        }
        public async Task<APIResponse> TraLoi(string userInput)
        {
            APIResponse response1 = new APIResponse();
            try
            {
                string Openning = "";
                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = $"{Openning} + {userInput}.\n"

                                }
                            }
                        }
                    }
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string answer = responseObject?.candidates[0].content?.parts[0]?.text ?? "Xin lỗi, câu hỏi của bạn đã vi phạm chính sách của Google hoặc câu trở lời quá dài nên Rem không hiển thị cho bạn được";
                    response1.ResponseCode = 201;
                    response1.Result = answer.ToString();
                }
            }
            catch (Exception ex)
            {
                response1.ResponseCode = 400;
                response1.ErrorMessage = ex.Message;
            }
            return response1;
        }
        public async Task<APIResponse> Response(RequestGeminiHinhAnh? info)
        {
            APIResponse response1 = new APIResponse();
            try
            {
                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;

                var parts = new List<object>
                {
                    new { text = info.CauHoi }
                };
                if (info.HinhAnh != null && info.HinhAnh.Count > 0)
                {
                    foreach (var imageBytes in info.HinhAnh)
                    {
                        string base64Image = Convert.ToBase64String(imageBytes);
                        parts.Add(new
                        {
                            inline_data = new
                            {
                                mime_type = "image/png",
                                data = base64Image
                            }
                        });
                    }
                }
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = parts.ToArray()
                        }
                    },
                    generationConfig = new
                    {
                        responseModalities = new[] { "Text", "Image" }
                    }
                };
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp-image-generation:generateContent?key={GoogleAPIKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                    string imageBase64 = responseObject?.candidates[0]?.content?.parts[0]?.inlineData?.data;
                    if (string.IsNullOrEmpty(imageBase64))
                    {
                        response1.ResponseCode = 400;
                        response1.ErrorMessage = "Không nhận được hình ảnh từ API.";
                    }
                    else
                    {
                        response1.ResponseCode = 201;
                        response1.Result = imageBase64;
                    }
                }
            }
            catch (Exception ex)
            {
                response1.ResponseCode = 400;
                response1.ErrorMessage = ex.Message;
            }
            return response1;
        }

        public async Task<APIResponse> PhanLoaiGopY(string noiDung)
        {
            APIResponse response = new APIResponse();
            try
            {
                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                var prompt = $"Phân loại nội dung sau thành 'tích cực', 'tiêu cực' hoặc 'bình thường': {noiDung}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var apiResponse = await client.PostAsync(
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}",
                        content
                    );
                    var responseString = await apiResponse.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                    string result = responseObject?.candidates[0].content?.parts[0]?.text ?? "bình thường";
                    response.ResponseCode = 201;
                    response.Result = result.Trim().ToLower();
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
        public async Task<APIResponse> TraLoiUpgrade(string userInput)
        {
            APIResponse response1 = new APIResponse();
            try
            { 

                string Openning = "Hãy dựa vào câu hỏi của khách hàng mà hãy phân loại câu hỏi giúp tôi:\n1. Nếu câu hỏi là về thông tin của 1 sản phẩm nào đó," +
                    " hãy xuất ra dữ liệu có định dạng là \"SP,Tên sản phẩm người dùng hỏi\". Ví dụ: \"SP,Quần\".\n2. Nếu câu hỏi là về việc thêm sản phẩm" +
                    " vào giỏ hàng và đầy đủ thông tin về số lượng mà mã sản phẩm thì xuất ra dữ liệu có định dạng là \"GH,Mã sản phẩm, Số Lượng\". " +
                    " Ví dụ: \"GH,A00001,1\" Còn nếu khách hàng Gửi thiếu thông tin thì hãy trả lời \"GHExcept\"";
                    
                var GoogleAPIKey = _authSettings.Google.GoogleAPIKey;
                var GoogleAPIUrl = _authSettings.Google.GoogleAPIUrl;

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = $"{Openning} + {userInput}.\n"

                                }
                            }
                        }
                    }
                };

                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={GoogleAPIKey}", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string answer = responseObject?.candidates[0].content?.parts[0]?.text ?? "Xin lỗi, câu hỏi của bạn đã vi phạm chính sách của Google hoặc câu trở lời quá dài nên Rem không hiển thị cho bạn được";
                    response1.ResponseCode = 201;
                    response1.Result = answer.ToString();
                }
            }
            catch (Exception ex)
            {
                response1.ResponseCode = 400;
                response1.ErrorMessage = ex.Message;
            }
            return response1;
        }
    }
}
