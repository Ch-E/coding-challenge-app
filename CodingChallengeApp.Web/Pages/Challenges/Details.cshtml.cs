using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using CodingChallengeApp.Web.Models;

namespace CodingChallengeApp.Web.Pages.Challenges
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public new ChallengeDto? Challenge { get; set; }
        public string? ErrorMessage { get; set; }
        public SubmissionResultDto? SubmissionResult { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please provide a solution")]
        public string? Solution { get; set; }

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadChallenge(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await LoadChallenge(id);

            if (!ModelState.IsValid || Challenge == null)
            {
                return Page();
            }

            try
            {
                // User must be authenticated to submit a solution
                if (User.Identity?.IsAuthenticated != true)
                {
                    return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("./Details", new { id }) });
                }

                var token = User.FindFirstValue("jwt");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToPage("/Account/Login", new { returnUrl = Url.Page("./Details", new { id }) });
                }

                var client = _httpClientFactory.CreateClient("ApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsJsonAsync($"api/challenges/{id}/submit", Solution);

                if (response.IsSuccessStatusCode)
                {
                    SubmissionResult = await response.Content.ReadFromJsonAsync<SubmissionResultDto>();
                    return Page();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to submit solution: {content}";
                    _logger.LogWarning("Failed to submit solution. Status code: {StatusCode}, Content: {Content}", 
                        response.StatusCode, content);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while submitting your solution. Please try again later.";
                _logger.LogError(ex, "Error submitting solution for challenge {ChallengeId}", id);
            }

            return Page();
        }

        private async Task LoadChallenge(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                
                // Add authorization if user is authenticated
                var token = User.FindFirstValue("jwt");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"api/challenges/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    Challenge = await response.Content.ReadFromJsonAsync<ChallengeDto>();
                }
                else
                {
                    ErrorMessage = response.StatusCode == System.Net.HttpStatusCode.NotFound
                        ? "Challenge not found or not available yet."
                        : $"Failed to load challenge. Status code: {response.StatusCode}";
                    
                    _logger.LogWarning("Failed to load challenge {ChallengeId}. Status code: {StatusCode}", 
                        id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading the challenge. Please try again later.";
                _logger.LogError(ex, "Error loading challenge {ChallengeId}", id);
            }
        }
    }
} 