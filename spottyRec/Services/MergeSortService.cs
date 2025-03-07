using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using spottyRec.Models;
using System.Collections.Generic;
using System.Text;

namespace spottyRec.Services
{
    public class MergeSortService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendApiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public MergeSortService(string backendApiBaseUrl)
        {
            _httpClient = new HttpClient();
            _backendApiBaseUrl = backendApiBaseUrl ?? throw new ArgumentNullException(nameof(backendApiBaseUrl));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<List<RecommendationDetails>> SortRecommendationsAsync(List<RecommendationDetails> recommendations)
        {
            if (recommendations == null || recommendations.Count == 0)
                return new List<RecommendationDetails>();

            string requestUrl = $"{_backendApiBaseUrl}/recommendations/sort";

            try
            {
                // Log the serialized request for debugging
                var serializedRequest = JsonSerializer.Serialize(recommendations, _jsonOptions);
                Console.WriteLine($"Sending request: {serializedRequest}");

                var content = new StringContent(
                    serializedRequest,
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log the response for debugging
                Console.WriteLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error from server: {response.StatusCode}");
                    Console.WriteLine($"Error content: {responseContent}");
                    return recommendations;
                }

                try
                {
                    var sortedRecommendations = JsonSerializer.Deserialize<List<RecommendationDetails>>(
                        responseContent,
                        _jsonOptions
                    );

                    if (sortedRecommendations == null)
                    {
                        Console.WriteLine("Deserialization resulted in null");
                        return recommendations;
                    }

                    return sortedRecommendations;
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"JSON Deserialization error: {jsonEx.Message}");
                    Console.WriteLine($"Response content that failed to deserialize: {responseContent}");
                    return recommendations;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sorting recommendations: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return recommendations;
            }
        }
    }
}