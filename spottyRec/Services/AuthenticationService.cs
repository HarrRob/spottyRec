using System.Diagnostics;

namespace spottyRec.Services
{
    public class AuthenticationService
    {
        private readonly string _clientId;
        private readonly string _redirectUri;

        public AuthenticationService(string clientId, string redirectUri)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _redirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
        }
        public void login()
        {
            // Define the required scopes
            string scopes = string.Join(" ", new[] {
                "user-top-read",
                "playlist-modify-public",
                "playlist-modify-private",
                "user-read-private",
                "user-read-email",
                "user-library-read",
                "user-library-modify",
                "user-follow-read",
                "user-follow-modify",
                "user-read-currently-playing",
                "playlist-read-private",
                "app-remote-control"
            });

            string authorizationUrl = $"https://accounts.spotify.com/authorize" +
                $"?response_type=code" +
                $"&client_id={_clientId}" +
                $"&scope={Uri.EscapeDataString(scopes)}" +
                $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                $"&show_dialog=true";

            // Open the browser to the authorization URL
            try
            {
                Console.WriteLine("Opening browser to authorize...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = authorizationUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening browser: {ex.Message}");
                Console.WriteLine($"Please manually open this URL: {authorizationUrl}");
            }
        }
    }

}
