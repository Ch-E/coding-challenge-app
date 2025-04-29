using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CodingChallengeApp.Api.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string token);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ISendGridClient _sendGridClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var apiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid API key is not configured");
                throw new InvalidOperationException("SendGrid API key is not configured");
            }
            _logger.LogInformation("Initializing SendGrid client with API key: {ApiKeyPrefix}...", apiKey.Substring(0, 5));
            _sendGridClient = new SendGridClient(apiKey);
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            try
            {
                _logger.LogInformation("=== Starting email verification process ===");
                _logger.LogInformation("Preparing to send verification email to {Email}", email);
                var fromEmail = _configuration["SendGrid:FromEmail"];
                if (string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogError("Sender email is not configured");
                    throw new InvalidOperationException("Sender email is not configured");
                }
                _logger.LogInformation("Using sender email: {FromEmail}", fromEmail);
                _logger.LogInformation("API Key prefix: {ApiKeyPrefix}", _configuration["SendGrid:ApiKey"]?.Substring(0, 5));

                var from = new EmailAddress(fromEmail, "Coding Challenge App");
                var to = new EmailAddress(email);
                var subject = "Verify your email address";
                
                // Get the base URL from configuration, with a fallback to the current request's host
                var baseUrl = _configuration["AppUrl"] ?? "https://localhost:7295";
                var verificationUrl = $"{baseUrl}/api/auth/verify-email?token={token}";
                
                _logger.LogInformation("Verification URL: {VerificationUrl}", verificationUrl);
                _logger.LogInformation("Base URL from config: {BaseUrl}", baseUrl);
                
                var htmlContent = $@"
                    <h2>Welcome to Coding Challenge App!</h2>
                    <p>Please verify your email address by clicking the link below:</p>
                    <p><a href='{verificationUrl}'>Verify Email</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't create an account, you can safely ignore this email.</p>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                _logger.LogInformation("Sending email via SendGrid...");
                
                try
                {
                    var response = await _sendGridClient.SendEmailAsync(msg);
                    _logger.LogInformation("SendGrid response status code: {StatusCode}", response.StatusCode);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        _logger.LogInformation("Verification email sent successfully to {Email}", email);
                    }
                    else
                    {
                        var responseBody = await response.Body.ReadAsStringAsync();
                        _logger.LogError("Failed to send verification email to {Email}. Status code: {StatusCode}, Response: {ResponseBody}", 
                            email, response.StatusCode, responseBody);
                        
                        // Check for specific SendGrid error codes
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            _logger.LogError("SendGrid API key is invalid or has insufficient permissions");
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            _logger.LogError("Sender email {FromEmail} is not verified in SendGrid", fromEmail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during SendGrid API call");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to {Email}", email);
                throw;
            }
        }
    }
} 