using Microsoft.AspNetCore.Mvc;
using spottyRec.Models;
using System.Text.Json;

namespace spottyRec.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        [HttpGet("callback")]
        public IActionResult Login([FromQuery] string code)
        {
            using var httpClient = new HttpClient();
            var response = httpClient.GetAsync("https://localhost:7035/auth/login?code="+code).Result;
            var tokenData = JsonSerializer.Deserialize<TokenDetails>(response.Content.ReadAsStringAsync().Result);
            HttpContext.Session.SetString("token", tokenData.AccessToken);
            return RedirectToPage("/Information");
        }
    }
}
