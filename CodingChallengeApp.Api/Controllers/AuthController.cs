using System;
using System.Threading.Tasks;
using CodingChallengeApp.Api.Models;
using CodingChallengeApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodingChallengeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        public async Task<ActionResult> VerifyEmail([FromQuery] string token)
        {
            var success = await _authService.VerifyEmailAsync(token);
            if (success)
            {
                return Ok(new { message = "Email verified successfully" });
            }
            return BadRequest(new { message = "Invalid or expired verification token" });
        }
    }
} 