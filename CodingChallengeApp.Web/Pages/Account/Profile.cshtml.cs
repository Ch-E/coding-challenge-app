using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace CodingChallengeApp.Web.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProfileModel> _logger;

        public UserDto? UserInfo { get; set; }
        public UserStatsDto? Stats { get; set; }
        public List<SubmissionDto>? Submissions { get; set; }

        public ProfileModel(IHttpClientFactory httpClientFactory, ILogger<ProfileModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                
                // Get JWT token from claims
                var token = User.FindFirstValue("jwt");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    // Get user profile information
                    var userResponse = await client.GetAsync("api/users/profile");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        UserInfo = await userResponse.Content.ReadFromJsonAsync<UserDto>();
                    }

                    // Get user stats
                    var statsResponse = await client.GetAsync("api/users/stats");
                    if (statsResponse.IsSuccessStatusCode)
                    {
                        Stats = await statsResponse.Content.ReadFromJsonAsync<UserStatsDto>();
                    }

                    // Get recent submissions
                    var submissionsResponse = await client.GetAsync("api/users/submissions");
                    if (submissionsResponse.IsSuccessStatusCode)
                    {
                        Submissions = await submissionsResponse.Content.ReadFromJsonAsync<List<SubmissionDto>>();
                    }
                }
                else
                {
                    return RedirectToPage("/Account/Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
            }

            return Page();
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalSubmissions { get; set; }
        public int CorrectSubmissions { get; set; }
        public decimal SuccessRate => TotalSubmissions > 0 ? (decimal)CorrectSubmissions / TotalSubmissions : 0;
        public int EasyCount { get; set; }
        public int MediumCount { get; set; }
        public int HardCount { get; set; }
        public int Rank { get; set; }
    }

    public class SubmissionDto
    {
        public int Id { get; set; }
        public int ChallengeId { get; set; }
        public string ChallengeTitle { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Code { get; set; } = string.Empty;
    }
} 