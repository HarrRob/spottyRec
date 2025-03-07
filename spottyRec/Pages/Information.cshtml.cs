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

public class InformationModel : PageModel
{
    private readonly TrackService _trackService;
    private readonly ReccomendationService _reccomendationService;
    private readonly PlaylistService _playlistService;
    private readonly ListeningHabitsService _listeningHabitsService;
    private readonly MergeSortService _mergeSortService;

    public InformationModel()
    {
        // Instantiate services; adjust the base URLs as needed
        _trackService = new TrackService("https://localhost:7035");
        _reccomendationService = new ReccomendationService("https://localhost:7035");
        _playlistService = new PlaylistService("https://localhost:7035");
        _listeningHabitsService = new ListeningHabitsService("https://localhost:7035"); 
        _mergeSortService = new MergeSortService("https://localhost:7035");
    }


    [BindProperty]
    public List<TrackDetails> Tracks { get; set; } = new List<TrackDetails>();

    public List<RecommendationDetails> Recommendations { get; set; } = new List<RecommendationDetails>();

    public ListeningHabitsDetails ListeningHabits { get; set; }

    public bool ShowTopTracks { get; set; } = false;
    public bool ShowRecommendations { get; set; } = false;
    public bool ShowListeningHabits { get; set; } = false;
    

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
        int limit = 10;
        int offset = 0;

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
        int limit = 10;
        int offset = 0;

        if (string.IsNullOrEmpty(Token))
        {
            ModelState.AddModelError(string.Empty, "Access token is missing.");
            return BadRequest();
        }

        // Optionally refresh Tracks if needed:
        Tracks = await _trackService.GetTrackDetailsAsync(Token);

        try
        {
            List<RecommendationDetails> allRecommendations = new List<RecommendationDetails>();

            // For each track, fetch recommendations and add them
            foreach (TrackDetails topTrack in Tracks)
            {
                RecommendationDetails recommendations = await _reccomendationService.GetRecommendationsAsync(
                    topTrack.trackID,
                    topTrack.genre,
                    topTrack.danceability,
                    topTrack.energy,
                    topTrack.tempo,
                    topTrack.valence
                );

                allRecommendations.Add(recommendations);
            }

            var sortedRecommendations = await _mergeSortService.SortRecommendationsAsync(allRecommendations);
            Recommendations = sortedRecommendations;


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
            // Create playlist name based on the source
            string playlistName = $"{source} Playlist";

            // Call the updated CreatePlaylistAsync which now returns a playlist ID
            var playlistId = await _playlistService.CreatePlaylistAsync(
                token: Token,
                playlistName: playlistName,
                trackIds: Tracks.Select(t => t.trackID).ToList()
            );

            if (string.IsNullOrEmpty(playlistId))
            {
                ModelState.AddModelError(string.Empty, "Failed to create playlist.");
                return BadRequest();
            }
            return null;
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

    public async Task<IActionResult> OnPostListeningHabits()
    {
        Token = HttpContext.Session.GetString("token") ?? Request.Query["accessToken"];

        if (string.IsNullOrEmpty(Token))
        {
            ModelState.AddModelError(string.Empty, "Access token is missing.");
            return BadRequest();
        }

        try
        {
            ListeningHabits = await _listeningHabitsService.GetListeningHabitsAsync(Token);

            if (ListeningHabits == null)
            {
                ModelState.AddModelError(string.Empty, "Could not retrieve listening habits. Returned object is null.");
            }
            else
            {
                HttpContext.Session.SetString("ListeningHabits", JsonSerializer.Serialize(ListeningHabits));
                ShowListeningHabits = true;
            }
        }
        catch (Exception ex)
        {
            // Log the full exception details
            Console.WriteLine($"Full Exception in OnPostListeningHabits: {ex}");

            ModelState.AddModelError(string.Empty, $"Detailed Error fetching listening habits: {ex.Message}");

            // If you want to see the full stack trace in the error
            if (ex.InnerException != null)
            {
                ModelState.AddModelError(string.Empty, $"Inner Exception: {ex.InnerException.Message}");
            }

            return StatusCode(500);
        }

        return Page();
    }



}
