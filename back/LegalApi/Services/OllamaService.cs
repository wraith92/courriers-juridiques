using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace LegalApi.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;

        public OllamaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:11434/");
        }

        public async Task<string> GenerateResponse(string prompt)
        {
            var request = new
            {
                model = "mistral", // ou llama2, selon ce que tu as installé
                prompt = prompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync("api/generate", request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("response").GetString() ?? "";
        }
    }
}
