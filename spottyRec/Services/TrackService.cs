using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using spottyRec.Models;
using System.Collections.Generic;

namespace spottyRec.Services
{
    public class TrackService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendApiBaseUrl;

        public TrackService(string backendApiBaseUrl)
        {
            _httpClient = new HttpClient();
            _backendApiBaseUrl = backendApiBaseUrl ?? throw new ArgumentNullException(nameof(backendApiBaseUrl));
        }

        // Method to fetch track details from the backend API using the provided token
        public async Task<List<TrackDetails>> GetTrackDetailsAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.", nameof(token));

            string requestUrl = $"{_backendApiBaseUrl}/toptracks/details?token={token}"; // Backend API URL for track details

            try
            {
                // Call the backend API to fetch track details
                var response = await _httpClient.GetAsync(requestUrl);

                // Read and deserialize the JSON response into a list of TrackDetails
                var responseContent = await response.Content.ReadAsStringAsync();
                var trackDetails = JsonSerializer.Deserialize<List<TrackDetails>>(responseContent);

                return trackDetails ?? new List<TrackDetails>();  // Return the track details, or an empty list if none
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching track details: {ex.Message}");
                return new List<TrackDetails>();  // Return an empty list in case of failure
            }
        }
    }
}
