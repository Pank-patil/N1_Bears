using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unique.Models;
using Microsoft.Extensions.Configuration;

namespace Unique.Controllers
{
    public class TwitterController : Controller
    {
        private readonly string clientId;
        private readonly string redirectUri;
        private readonly ApplicationDbContext _context;

        public TwitterController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            clientId = config["Twitter:ClientId"];
            redirectUri = config["Twitter:RedirectUri"];

            if (string.IsNullOrEmpty(clientId))
                throw new InvalidOperationException("Twitter ClientId is not configured.");
            if (string.IsNullOrEmpty(redirectUri))
                throw new InvalidOperationException("Twitter RedirectUri is not configured.");
        }

        public IActionResult ConnectX()
        {
            var isVerified = HttpContext.Session.GetString("TwitterVerified");
            ViewBag.TwitterVerified = isVerified == "true";
            return View();
        }

        public IActionResult JoinWL()
        {
            var isVerified = HttpContext.Session.GetString("TwitterVerified");
            ViewBag.TwitterVerified = isVerified == "true";
            return View();
        }

        public IActionResult Login()
        {
            var state = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("state", state);

            var codeVerifier = GenerateCodeVerifier();
            HttpContext.Session.SetString("code_verifier", codeVerifier);

            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            var authUrl = $"https://twitter.com/i/oauth2/authorize" +
                          $"?response_type=code" +
                          $"&client_id={Uri.EscapeDataString(clientId)}" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                          $"&scope=tweet.read%20users.read%20follows.read%20offline.access" +
                          $"&state={Uri.EscapeDataString(state)}" +
                          $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                          $"&code_challenge_method=S256";

            return Redirect(authUrl);
        }

        public async Task<IActionResult> Callback(string code, string state)
        {
            try
            {
                var savedState = HttpContext.Session.GetString("state");
                if (state != savedState)
                    return BadRequest("Invalid state parameter.");

                var codeVerifier = HttpContext.Session.GetString("code_verifier");

                using var httpClient = new System.Net.Http.HttpClient();

                var tokenResponse = await httpClient.PostAsync("https://api.twitter.com/2/oauth2/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "code", code },
                        { "grant_type", "authorization_code" },
                        { "client_id", clientId },
                        { "redirect_uri", redirectUri },
                        { "code_verifier", codeVerifier }
                    }));

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                    return Content($"Token request failed: {tokenResponse.StatusCode} - {errorContent}");
                }

                var tokenJson = JObject.Parse(await tokenResponse.Content.ReadAsStringAsync());
                var accessToken = tokenJson["access_token"]?.ToString();

                if (string.IsNullOrEmpty(accessToken))
                    return Content("Access token missing in token response.");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var userResponse = await httpClient.GetAsync("https://api.twitter.com/2/users/me");

                if (!userResponse.IsSuccessStatusCode)
                {
                    var errorContent = await userResponse.Content.ReadAsStringAsync();
                    return Content($"User info request failed: {userResponse.StatusCode} - {errorContent}");
                }

                var userJson = JObject.Parse(await userResponse.Content.ReadAsStringAsync());
                var userId = userJson["data"]?["id"]?.ToString();

                if (string.IsNullOrEmpty(userId))
                    return Content("User ID missing in user info response.");

                HttpContext.Session.SetString("TwitterVerified", "true");

                return RedirectToAction("JoinWL", "Home"); // because JoinWL is in HomeController

            }
            catch (Exception ex)
            {
                return Content($"Exception occurred in Callback: {ex.Message}\n{ex.StackTrace}");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(WhitelistEntry entry)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    entry.ReferralCode = Guid.NewGuid().ToString("N").Substring(0, 8);

                    _context.WhitelistEntries.Add(entry);
                    _context.SaveChanges();

                    ViewBag.BaseUrl = $"{Request.Scheme}://{Request.Host}";

                    return View("Success", entry);
                }

                return View(entry);
            }
            catch (Exception ex)
            {
                return Content($"Exception occurred while applying: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Helpers for PKCE
        private string GenerateCodeVerifier()
        {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            var bytes = Encoding.ASCII.GetBytes(codeVerifier);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return Base64UrlEncode(hash);
        }

        private string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
