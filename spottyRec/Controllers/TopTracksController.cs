using Microsoft.AspNetCore.Mvc;
using spottyRec.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace spottyRec.Controllers
{
    [Route("tracks")]
    [ApiController]
    public class TopTracksFrontendController : ControllerBase
    {
        [HttpGet("user")]
        public async Task<IActionResult> GetUserTopTracks(
            [FromQuery] string accessToken,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required.");
            }

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://localhost:7035/api/top-tracks?accessToken={accessToken}&limit={limit}&offset={offset}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching top tracks.");
            }

            var topTracks = JsonSerializer.Deserialize<object>(await response.Content.ReadAsStringAsync());

            // 🔄 Pass the tracks to a Razor Page
            return RedirectToPage("/TopTracks", new { tracks = JsonSerializer.Serialize(topTracks) });
        }
    }
}
