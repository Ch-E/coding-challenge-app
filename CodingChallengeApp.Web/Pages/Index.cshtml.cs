using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Text.Json;
using CodingChallengeApp.Web.Models;

namespace CodingChallengeApp.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public ChallengeDto? TodayChallenge { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("api/challenges/today");
            
            if (response.IsSuccessStatusCode)
            {
                TodayChallenge = await response.Content.ReadFromJsonAsync<ChallengeDto>();
            }
            else
            {
                _logger.LogWarning("Failed to retrieve today's challenge. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving today's challenge");
        }
    }
}
