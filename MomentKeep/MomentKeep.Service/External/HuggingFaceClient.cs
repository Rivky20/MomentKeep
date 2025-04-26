using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using MomentKeep.Core.Interfaces.External;

namespace MomentKeep.Service.External
{
    public class HuggingFaceClient : IHuggingFaceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public HuggingFaceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["HuggingFace:ApiKey"];
            _apiUrl = configuration["HuggingFace:ApiUrl"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl))
            {
                throw new ArgumentException("Hugging Face API configuration is missing");
            }

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<byte[]> GenerateImageAsync(string prompt, string style = null, string size = "512x512")
        {
            var requestData = new
            {
                inputs = style != null ? $"{prompt} in {style} style" : prompt,
                parameters = new
                {
                    return_type = "binary",
                    img_size = size
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(_apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Hugging Face API request failed: {response.StatusCode}. Details: {errorContent}");
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}