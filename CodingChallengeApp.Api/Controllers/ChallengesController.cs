using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CodingChallengeApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace CodingChallengeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChallengesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetChallenges()
        {
            var challenges = await _context.Challenges.ToListAsync();
            return Ok(challenges);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallenge(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
            {
                return NotFound();
            }
            return Ok(challenge);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallenge(Challenge challenge)
        {
            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetChallenge), new { id = challenge.Id }, challenge);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChallenge(int id, Challenge challenge)
        {
            if (id != challenge.Id)
            {
                return BadRequest();
            }

            _context.Entry(challenge).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallenge(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
            {
                return NotFound();
            }

            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("today")]
        // Temporarily removed for testing
        // [Authorize]
        public async Task<IActionResult> GetTodayChallenge()
        {
            // Placeholder: fetch today's challenge
            var today = DateTime.UtcNow.Date;
            var challenge = await _context.Challenges.FirstOrDefaultAsync(c => c.DateAvailable == today);
            if (challenge == null) return NotFound();
            return Ok(challenge);
        }

        [HttpPost("{id}/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitSolution(int id, [FromBody] string code)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new InvalidOperationException("User ID not found in claims");
            var userIdInt = int.Parse(userId);
            var user = await _context.Users.FindAsync(userIdInt);
            var challenge = await _context.Challenges.FindAsync(id);
            
            if (challenge == null) return NotFound();
            if (user == null) return BadRequest("User not found");

            // For now, treat submitted code as output and compare to sample output
            bool isCorrect = code.Trim() == (challenge.SampleOutput?.Trim() ?? "");

            var submission = new Submission
            {
                UserId = userIdInt,
                ChallengeId = id,
                Code = code,
                IsCorrect = isCorrect,
                SubmittedAt = DateTime.UtcNow,
                User = user,
                Challenge = challenge
            };
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return Ok(new { isCorrect, message = isCorrect ? "Correct!" : "Incorrect." });
        }
    }
} 