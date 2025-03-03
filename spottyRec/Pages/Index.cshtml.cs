using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using spottyRec.Services;

namespace spottyRec.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel>
    _logger;
        private readonly AuthenticationService _authenticationService;
        private readonly string CLIENT_ID = "f9a18ea2ae15426a8665d4df92feb418";
        private readonly string REDIRECT_URI = "https://localhost:7242/auth/callback";

        public IndexModel(ILogger<IndexModel>
            logger)
        {
            _logger = logger;
            _authenticationService = new AuthenticationService(CLIENT_ID, REDIRECT_URI);
        }

        public void OnGet()
        {

        }
        public void OnPostLogin()
        {
            _authenticationService.login();
        }
    }
}