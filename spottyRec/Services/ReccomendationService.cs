using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using spottyRec.Models;
using System.Collections.Generic;

namespace spottyRec.Services
{
    public class ReccomendationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendApiBaseUrl;

        public ReccomendationService(string backendApiBaseUrl)
        {
            _httpClient = new HttpClient();
            _backendApiBaseUrl = backendApiBaseUrl ?? throw new ArgumentNullException(nameof(backendApiBaseUrl));
        }

        // Method to fetch recommendations from the backend API
        public async Task<Recommendation> GetRecommendationsAsync(
            string targetTrackID,
            string targetGenre,
            float targetDanceability,
            float targetEnergy,
            float targetTempo,
            float targetValence,
            int limit = 10)
        {
            if (string.IsNullOrEmpty(targetTrackID) || string.IsNullOrEmpty(targetGenre))
                throw new ArgumentException("Track ID and genre are required.");

            string requestUrl = $"{_backendApiBaseUrl}/reccomendations/details" +
                                $"?targetTrackID={targetTrackID}" +
                                $"&targetGenre={targetGenre}" +
                                $"&targetDanceability={targetDanceability}" +
                                $"&targetEnergy={targetEnergy}" +
                                $"&targetTempo={targetTempo}" +
                                $"&targetValence={targetValence}" +
                                $"&limit={limit}";  // Backend API URL for recommendations

            try
            {
                // Call the backend API to fetch recommendations
                var response = await _httpClient.GetAsync(requestUrl);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the JSON response into a list of Recommendation objects
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var recommendations = JsonSerializer.Deserialize<Recommendation>(responseContent,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                    return recommendations ?? null;  // Return the recommendations, or an empty list if none
                }
                else
                {
                    Console.WriteLine("Error fetching recommendations: " + response.ReasonPhrase);
                    return null;  // Return an empty list in case of failure
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recommendations: {ex.Message}");
                return null;  // Return an empty list in case of failure
            }
        }
    }
}
