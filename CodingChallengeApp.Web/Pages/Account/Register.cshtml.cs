using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace CodingChallengeApp.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public RegisterInputModel RegisterInput { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public RegisterModel(IHttpClientFactory httpClientFactory, ILogger<RegisterModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("ApiClient");
                    var response = await client.PostAsJsonAsync("api/auth/register", new
                    {
                        RegisterInput.Username,
                        RegisterInput.Email,
                        RegisterInput.Password
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToPage("./Login", new { registered = true });
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        
                        try
                        {
                            var jsonDoc = JsonDocument.Parse(content);
                            if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                            {
                                ErrorMessage = messageElement.GetString();
                            }
                            else
                            {
                                ErrorMessage = $"Registration failed: {content}";
                            }
                        }
                        catch (JsonException)
                        {
                            ErrorMessage = $"Registration failed: {content}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration");
                    ErrorMessage = "An error occurred during registration. Please try again later.";
                }
            }

            return Page();
        }
    }

    public class RegisterInputModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
} 