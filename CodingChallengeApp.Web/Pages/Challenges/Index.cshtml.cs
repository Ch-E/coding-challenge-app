using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace CodingChallengeApp.Web.Pages.Challenges
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        public List<ChallengeDto>? Challenges { get; set; }
        public string? ErrorMessage { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? DifficultyFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool Today { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
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
                }

                HttpResponseMessage response;
                
                if (Today)
                {
                    // Get today's challenge
                    response = await client.GetAsync("api/challenges/today");
                    if (response.IsSuccessStatusCode)
                    {
                        var todayChallenge = await response.Content.ReadFromJsonAsync<ChallengeDto>();
                        Challenges = new List<ChallengeDto> { todayChallenge };
                    }
                }
                else
                {
                    // Get all challenges with optional filter
                    string url = "api/challenges";
                    if (!string.IsNullOrEmpty(DifficultyFilter))
                    {
                        url += $"?difficulty={DifficultyFilter}";
                    }
                    
                    response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        Challenges = await response.Content.ReadFromJsonAsync<List<ChallengeDto>>();
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Failed to load challenges. Status code: {response.StatusCode}";
                    _logger.LogWarning("Failed to load challenges. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load challenges. Please try again later.";
                _logger.LogError(ex, "Error loading challenges");
            }
        }
    }

    public class ChallengeDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public DateTime DateAvailable { get; set; }
        public string? SampleInput { get; set; }
        public string? SampleOutput { get; set; }
    }
} 