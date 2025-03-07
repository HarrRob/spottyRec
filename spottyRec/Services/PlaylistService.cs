using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace spottyRec.Services
{
    public class PlaylistService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendApiBaseUrl;

        public PlaylistService(string backendApiBaseUrl)
        {
            _httpClient = new HttpClient();
            _backendApiBaseUrl = backendApiBaseUrl ?? throw new ArgumentNullException(nameof(backendApiBaseUrl));
        }

        // Method to create a playlist
        public async Task<string> CreatePlaylistAsync(string token, string playlistName, List<string> trackIds)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.");
            if (string.IsNullOrEmpty(playlistName))
                throw new ArgumentException("Playlist name is required.");
            if (trackIds == null || trackIds.Count == 0)
                throw new ArgumentException("Track IDs are required.");

            try
            {
                // Create the request body
                var requestBody = new
                {
                    PlaylistName = playlistName,
                    TrackIds = trackIds
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Build the URL for creating a playlist
                string requestUrl = $"{_backendApiBaseUrl}/playlists/create?token={token}";

                // Make the request to the backend
                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error creating playlist: {response.ReasonPhrase}. Response: {responseContent}");
                }

                // Deserialize the response to get the playlist ID
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return result.GetProperty("playlistId").GetString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create playlist: {ex.Message}", ex);
            }
        }
    }
}