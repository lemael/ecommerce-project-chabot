using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using EcommerceChatbot.Models;

namespace EcommerceChatbot.Services
{
    public class OpenRouterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenRouterService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenRouter:ApiKey"] ?? throw new ArgumentNullException("OpenRouter:ApiKey manquant dans la configuration.");
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",  // tu peux aussi essayer "gpt-4" si ta clé le permet
                messages = new[]
                {
                    new { role = "system", content = "Tu es un assistant e-commerce." },
                    new { role = "user", content = prompt }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenRouterResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Pas de réponse.";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Erreur OpenRouter: " + errorContent);
                return $"Erreur lors de la réponse OpenRouter: {errorContent}";
            }
        }
    }

}
