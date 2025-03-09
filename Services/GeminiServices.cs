using Newtonsoft.Json;
using UltraStrore.Helper;
using System.Text;
using UltraStrore.Repository;
using UltraStrore.Utils;

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
                string Openning = "Hãy đóng vai Rem và trả lời giúp tôi câu hỏi sau:";              
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


