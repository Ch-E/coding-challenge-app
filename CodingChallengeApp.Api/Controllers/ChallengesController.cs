using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CodingChallengeApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodingChallengeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ChallengesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("today")]
        [Authorize]
        public async Task<IActionResult> GetTodayChallenge()
        {
            // Placeholder: fetch today's challenge
            var today = DateTime.UtcNow.Date;
            var challenge = await _db.Challenges.FirstOrDefaultAsync(c => c.DateAvailable == today);
            if (challenge == null) return NotFound();
            return Ok(challenge);
        }

        [HttpPost("{id}/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitSolution(int id, [FromBody] string code)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _db.Users.FindAsync(userId);
            var challenge = await _db.Challenges.FindAsync(id);
            
            if (challenge == null) return NotFound();
            if (user == null) return BadRequest("User not found");

            // For now, treat submitted code as output and compare to sample output
            bool isCorrect = code.Trim() == (challenge.SampleOutput?.Trim() ?? "");

            var submission = new Submission
            {
                UserId = userId,
                ChallengeId = id,
                Code = code,
                IsCorrect = isCorrect,
                SubmittedAt = DateTime.UtcNow,
                User = user,
                Challenge = challenge
            };
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            return Ok(new { isCorrect, message = isCorrect ? "Correct!" : "Incorrect." });
        }
    }
} 