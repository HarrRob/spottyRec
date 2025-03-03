using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using spottyRec.Models;
using spottyRec.Services;
using System.Linq;
using System.Text.Json; // Needed for JSON serialization
using spottyRec.Models;
using spottyRec.Utils;

public class InformationModel : PageModel
{
    private readonly TrackService _trackService;
    private readonly ReccomendationService _reccomendationService;
    private readonly PlaylistService _playlistService; // Add PlaylistService here

    public InformationModel()
    {
        // Instantiate services; adjust the base URLs as needed
        _trackService = new TrackService("https://localhost:7035");
        _reccomendationService = new ReccomendationService("https://localhost:7035");
        _playlistService = new PlaylistService("https://localhost:7035");
    }


    [BindProperty]
    public List<TrackDetails> Tracks { get; set; } = new List<TrackDetails>();

    public List<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

    public bool ShowTopTracks { get; set; } = false;
    public bool ShowRecommendations { get; set; } = false;

    public string Token { get; set; }

    [BindProperty]
    public bool ShowCreatePlaylistButton
    {
        get => TempData.ContainsKey("ShowCreatePlaylistButton") && (bool)TempData["ShowCreatePlaylistButton"];
        set => TempData["ShowCreatePlaylistButton"] = value;
    }

    public async Task<IActionResult> OnPostTopTracks()
    {
        HttpContext.Session.SetString("TrackSource", "Top Tracks");
        ShowTopTracks = true;
        ShowRecommendations = false;
        ShowCreatePlaylistButton = true;  // Show Create Playlist button after fetching top tracks

        Token = HttpContext.Session.GetString("token") ?? Request.Query["accessToken"];

        if (string.IsNullOrEmpty(Token))
        {
            ModelState.AddModelError(string.Empty, "Access token is missing.");
            return BadRequest();
        }

        try
        {
            Tracks = await _trackService.GetTrackDetailsAsync(Token);

            if (Tracks == null || Tracks.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No matching tracks found in the local database.");
            }
            else
            {
                // Persist the tracks in Session (serialize them to JSON)
                HttpContext.Session.SetString("Tracks", JsonSerializer.Serialize(Tracks));
            }
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error fetching tracks: {ex.Message}");
            return StatusCode(500);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostReccomendation()
    {
        HttpContext.Session.SetString("TrackSource", "Recommendations");
        ShowRecommendations = true;
        ShowTopTracks = false;
        ShowCreatePlaylistButton = true;  // Show Create Playlist button after fetching recommendations

        Token = HttpContext.Session.GetString("token") ?? Request.Query["accessToken"];

        if (string.IsNullOrEmpty(Token))
        {
            ModelState.AddModelError(string.Empty, "Access token is missing.");
            return BadRequest();
        }

        // Optionally refresh Tracks if needed:
        Tracks = await _trackService.GetTrackDetailsAsync(Token);

        try
        {
            List<Recommendation> allRecommendations = new List<Recommendation>();

            // For each track, fetch recommendations and add them
            foreach (TrackDetails topTrack in Tracks)
            {
                Recommendation recommendations = await _reccomendationService.GetRecommendationsAsync(
                    topTrack.trackID,
                    topTrack.genre,
                    topTrack.danceability,
                    topTrack.energy,
                    topTrack.tempo,
                    topTrack.valence
                );

                allRecommendations.Add(recommendations);
            }

            Recommendations = MergeSortUtility.SortByScore(allRecommendations);

            Tracks = Recommendations.Select(rec => new TrackDetails
            {
                trackID = rec.TrackID,
                // Leave other properties with their default values
                title = string.Empty,
                artist = string.Empty,
                genre = string.Empty,
                year = 0,
                danceability = 0,
                energy = 0,
                tempo = 0,
                valence = 0
            }).ToList(); ;
            if (Tracks == null || Tracks.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No matching tracks found in the local database.");
            }
            else
            {
                // Persist the tracks in Session (serialize them to JSON)
                HttpContext.Session.SetString("Tracks", JsonSerializer.Serialize(Tracks));
            }

            if (Recommendations == null || Recommendations.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No recommendations found.");
            }
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error fetching recommendations: {ex.Message}");
            return StatusCode(500);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreatePlaylist()
    {
        // Retrieve stored data from session
        var tracksJson = HttpContext.Session.GetString("Tracks");
        Token = HttpContext.Session.GetString("token") ?? Request.Query["accessToken"];

        var source = HttpContext.Session.GetString("TrackSource") ?? "My";

        if (!string.IsNullOrEmpty(tracksJson))
        {
            Tracks = JsonSerializer.Deserialize<List<TrackDetails>>(tracksJson);
        }

        if (Tracks == null || Tracks.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "No tracks to add to the playlist.");
            return BadRequest();
        }

        try
        {
            // Retrieve the user ID from the backend
            var userId = await _playlistService.GetUserIdAsync(Token);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Failed to retrieve user ID.");
                return BadRequest();
            }

            // Create playlist name based on the source
            string playlistName = $"{source} Playlist";

            // Call CreatePlaylistAsync (which returns void)
            await _playlistService.CreatePlaylistAsync(userId, Token, playlistName, Tracks.Select(t => t.trackID).ToList());

            TempData["Message"] = $"{source} playlist created successfully!";
            return RedirectToPage("Success");
        }
        catch (HttpRequestException httpEx)
        {
            ModelState.AddModelError(string.Empty, $"HTTP Request Error: {httpEx.Message}");
            return StatusCode(500);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating playlist: {ex.Message}");
            return StatusCode(500);
        }
    }


}
