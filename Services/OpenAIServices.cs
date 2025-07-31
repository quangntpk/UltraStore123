using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UltraStrore.Repository;
using UltraStrore.Utils;

namespace UltraStrore.Services
{
    public class OpenAIServices : IOpenAIServices
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _openAISettings;

        public OpenAIServices(HttpClient httpClient, IOptions<OpenAISettings> openAISettings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _openAISettings = openAISettings?.Value ?? throw new ArgumentNullException(nameof(openAISettings));
        }

        public async Task<string> GetChatResponseAsync(string userQuery)
        {
            if (string.IsNullOrWhiteSpace(userQuery))
                throw new ArgumentException("Truy vấn không được để trống.", nameof(userQuery));

            var requestBody = new
            {
                model = _openAISettings.DefaultModel,
                messages = new[]
                {
                    new { role = "user", content = userQuery }
                },
                temperature = 1.0,
                top_p = 1.0
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAISettings.ApiKey}");

            var response = await _httpClient.PostAsync($"{_openAISettings.ApiUrl}/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Yêu cầu API OpenAI không thành công: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

            return responseObject.choices[0].message.content.ToString();
        }
    }
}