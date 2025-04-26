using System;

namespace CodingChallengeApp.Web.Models
{
    public class ChallengeDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public DateTime DateAvailable { get; set; }
        public string? SampleInput { get; set; }
        public string? SampleOutput { get; set; }
    }

    public class SubmissionResultDto
    {
        public bool IsCorrect { get; set; }
        public string? Message { get; set; }
    }
} 