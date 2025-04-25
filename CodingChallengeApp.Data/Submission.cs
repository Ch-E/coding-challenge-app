using System;

namespace CodingChallengeApp.Data
{
    public class Submission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ChallengeId { get; set; }
        public required string Code { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public required User User { get; set; }
        public required Challenge Challenge { get; set; }
    }
} 