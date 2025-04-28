using System;
using System.ComponentModel.DataAnnotations;

namespace CodingChallengeApp.Data
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public required string Username { get; set; }
        [Required, MaxLength(255)]
        public required string Email { get; set; }
        [Required, MaxLength(255)]
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
