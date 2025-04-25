﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CodingChallengeApp.Data
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Username { get; set; }
        [Required, MaxLength(255)]
        public string Email { get; set; }
        [Required, MaxLength(255)]
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
