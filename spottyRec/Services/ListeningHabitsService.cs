using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using spottyRec.Models;
namespace spottyRec.Services
{
    public class ListeningHabitsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendApiBaseUrl;
        public ListeningHabitsService(string backendApiBaseUrl)
        {
            _httpClient = new HttpClient();
            _backendApiBaseUrl = backendApiBaseUrl ?? throw new ArgumentNullException(nameof(backendApiBaseUrl));
        }
        public async Task<ListeningHabitsDetails> GetListeningHabitsAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.", nameof(token));

            string requestURL = $"{_backendApiBaseUrl}/listeninghabits/details?token={token}";
            try
            {
                var response = await _httpClient.GetAsync(requestURL);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var listeningHabits = JsonSerializer.Deserialize<ListeningHabitsDetails>(responseContent, options);
                return listeningHabits ?? new ListeningHabitsDetails();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching listening habits: {ex.Message}");
                return new ListeningHabitsDetails();  // Return an empty object in case of failure
            }
        }
    }
}