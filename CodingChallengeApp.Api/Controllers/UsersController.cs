using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CodingChallengeApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CodingChallengeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{id}/progress")]
        [Authorize]
        public async Task<IActionResult> GetUserProgress(int id)
        {
            // Placeholder: fetch user progress
            var submissions = await _db.Submissions
                .Where(s => s.UserId == id)
                .Include(s => s.Challenge)
                .ToListAsync();
            return Ok(submissions);
        }
    }
} 