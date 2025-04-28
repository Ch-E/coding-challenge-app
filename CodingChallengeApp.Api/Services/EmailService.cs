using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridClient = new SendGridClient(_configuration["SendGrid:ApiKey"]);
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], "Coding Challenge App");
            var to = new EmailAddress(email);
            var subject = "Verify your email address";
            var verificationUrl = $"{_configuration["AppUrl"]}/verify-email?token={token}";
            
            var htmlContent = $@"
                <h2>Welcome to Coding Challenge App!</h2>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify Email</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't create an account, you can safely ignore this email.</p>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            await _sendGridClient.SendEmailAsync(msg);
        }
    }
} 