using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CodingChallengeApp.Api.Models;
using CodingChallengeApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace CodingChallengeApp.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> VerifyEmailAsync(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("=== Starting user registration process ===");
                _logger.LogInformation("Checking if email {Email} is already registered", request.Email);
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    throw new Exception("Email already registered");
                }

                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = BC.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsEmailVerified = false,
                    EmailVerificationToken = Guid.NewGuid().ToString(),
                    EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
                };

                _logger.LogInformation("Adding new user with email {Email}", request.Email);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User saved successfully with ID {UserId}", user.Id);

                // Send verification email
                _logger.LogInformation("=== Attempting to send verification email ===");
                _logger.LogInformation("Email: {Email}, Token: {Token}", user.Email, user.EmailVerificationToken);
                try
                {
                    await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken);
                    _logger.LogInformation("Verification email sent successfully to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
                    // Don't throw the exception, just log it and continue
                }

                return GenerateAuthResponse(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email {Email}", request.Email);
                throw;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting login for email {Email}", request.Email);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BC.Verify(request.Password, user.PasswordHash))
                {
                    throw new Exception("Invalid email or password");
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} logged in successfully", user.Id);

                return GenerateAuthResponse(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", request.Email);
                throw;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                _logger.LogInformation("Attempting to verify email with token");
                var user = await _context.Users.FirstOrDefaultAsync(u => 
                    u.EmailVerificationToken == token && 
                    u.EmailVerificationTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    _logger.LogWarning("Invalid or expired verification token");
                    return false;
                }

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Email verified successfully for user {UserId}", user.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email with token");
                throw;
            }
        }

        private AuthResponse GenerateAuthResponse(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponse
            {
                Token = tokenHandler.WriteToken(token),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEmailVerified = user.IsEmailVerified
            };
        }
    }
} 