using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace CodingChallengeApp.Web.Pages
{
    public class LeaderboardModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LeaderboardModel> _logger;

        public List<LeaderboardEntryDto>? Leaderboard { get; set; }
        public string? ErrorMessage { get; set; }
        public int? CurrentUserId { get; set; }

        public LeaderboardModel(IHttpClientFactory httpClientFactory, ILogger<LeaderboardModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                
                // Add JWT token if user is authenticated
                var token = User.FindFirstValue("jwt");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    // Try to get current user ID
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(userIdClaim, out var userId))
                    {
                        CurrentUserId = userId;
                    }
                }

                var response = await client.GetAsync("api/leaderboard");
                
                if (response.IsSuccessStatusCode)
                {
                    Leaderboard = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
                }
                else
                {
                    ErrorMessage = $"Failed to load leaderboard. Status code: {response.StatusCode}";
                    _logger.LogWarning("Failed to load leaderboard. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load leaderboard. Please try again later.";
                _logger.LogError(ex, "Error loading leaderboard");
            }
        }
    }

    public class LeaderboardEntryDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int SolvedChallenges { get; set; }
        public int TotalSubmissions { get; set; }
        public decimal SuccessRate => TotalSubmissions > 0 ? (decimal)SolvedChallenges / TotalSubmissions : 0;
    }
} 