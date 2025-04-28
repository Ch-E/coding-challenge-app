using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CodingChallengeApp.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public LoginInputModel LoginInput { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public void OnGet(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewData["ReturnUrl"] = returnUrl;
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("ApiClient");
                    var response = await client.PostAsJsonAsync("api/auth/login", LoginInput);

                    if (response.IsSuccessStatusCode)
                    {
                        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

                        if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, LoginInput.Email),
                                new Claim("jwt", loginResponse.Token)
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = LoginInput.RememberMe,
                                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                            };

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);

                            return LocalRedirect(returnUrl);
                        }
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        ErrorMessage = response.StatusCode switch
                        {
                            System.Net.HttpStatusCode.Unauthorized => "Invalid email or password",
                            System.Net.HttpStatusCode.BadRequest => "Invalid login attempt",
                            _ => $"Login failed: {content}"
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login");
                    ErrorMessage = "An error occurred during login. Please try again later.";
                }
            }

            return Page();
        }
    }

    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
} 