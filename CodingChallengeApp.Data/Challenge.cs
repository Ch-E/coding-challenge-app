using System;
using System.ComponentModel.DataAnnotations;

namespace CodingChallengeApp.Data
{
    public class Challenge
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public required string Title { get; set; }
        public required string Description { get; set; }
        [MaxLength(50)]
        public required string Difficulty { get; set; }
        public DateTime DateAvailable { get; set; }
        public required string SampleInput { get; set; }
        public required string SampleOutput { get; set; }
    }
} 