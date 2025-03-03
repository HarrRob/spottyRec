﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace spottyRec.Services
{
    public class PlaylistService
    {
        private readonly HttpClient _httpClient;
        private readonly string _backendApiBaseUrl;

        // Constructor that accepts HttpClient and backend API URL
        public PlaylistService(string backendApiBaseUrl)
        {
            _httpClient = new HttpClient();
            _backendApiBaseUrl = backendApiBaseUrl ?? throw new ArgumentNullException(nameof(backendApiBaseUrl));
        }

        // Method to get the user ID from the backend API
        public async Task<string> GetUserIdAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.", nameof(token));

            string requestUrl = $"{_backendApiBaseUrl}/playlist/userid?token={token}";

            try
            {
                // Call the backend API to fetch the user ID
                var response = await _httpClient.GetAsync(requestUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the response content into a dictionary and extract the user ID
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
                return (result != null && result.ContainsKey("userId")) ? result["userId"] : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user ID: {ex.Message}");
                return string.Empty;
            }
        }

        // Method to create a playlist for a user
        public async Task CreatePlaylistAsync(string userId, string token, string playlistName, List<string> trackIds)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                throw new ArgumentException("User ID and token are required.");
            if (string.IsNullOrEmpty(playlistName))
                throw new ArgumentException("Playlist name is required.");

            // Build the URL for creating a playlist
            string requestUrl = $"{_backendApiBaseUrl}/playlist/createplaylist/{userId}";

            // Create the JSON payload for the new playlist
            var requestBody = new
            {
                name = playlistName,
                description = "Generated by SpottyRec",
                @public = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Set the authorization header (Spotify expects the token in the header)
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsync(requestUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error creating playlist: {response.ReasonPhrase}. Response: {responseContent}");
                    return;
                }

                // Optionally, if there are tracks to add...
                if (trackIds != null && trackIds.Count > 0)
                {
                    // Convert track IDs to Spotify URIs
                    var trackUris = trackIds.Select(id => $"spotify:track:{id}").ToList();
                    await AddTracksToPlaylistAsync(responseContent, token, trackUris);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating playlist: {ex.Message}");
            }
        }

        // Method to add tracks to a playlist
        private async Task AddTracksToPlaylistAsync(string playlistResponseContent, string token, List<string> trackUris)
        {
            try
            {
                // Deserialize the response to get the playlist ID
                var playlistResponse = JsonSerializer.Deserialize<JsonElement>(playlistResponseContent);
                var playlistId = playlistResponse.GetProperty("id").GetString();

                // Build the URL for adding tracks to the playlist
                string addTracksUrl = $"{_backendApiBaseUrl}/playlists/{playlistId}/tracks";

                var requestBody = new { uris = trackUris };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.PostAsync(addTracksUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error adding tracks: {response.ReasonPhrase}. Response: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding tracks to playlist: {ex.Message}");
            }
        }
    }
}
