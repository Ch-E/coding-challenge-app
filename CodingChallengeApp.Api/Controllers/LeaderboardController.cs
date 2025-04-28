using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CodingChallengeApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CodingChallengeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        public LeaderboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLeaderboard()
        {
            // Placeholder: leaderboard by correct submissions
            var leaderboard = await _db.Users
                .Select(u => new {
                    u.Id,
                    u.Username,
                    Solved = _db.Submissions.Count(s => s.UserId == u.Id && s.IsCorrect)
                })
                .OrderByDescending(x => x.Solved)
                .Take(20)
                .ToListAsync();
            return Ok(leaderboard);
        }
    }
} 