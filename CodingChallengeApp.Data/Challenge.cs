using System;
using System.ComponentModel.DataAnnotations;

namespace CodingChallengeApp.Data
{
    public class Challenge
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Title { get; set; }
        public string Description { get; set; }
        [MaxLength(50)]
        public string Difficulty { get; set; }
        public DateTime DateAvailable { get; set; }
        public string SampleInput { get; set; }
        public string SampleOutput { get; set; }
    }
} 