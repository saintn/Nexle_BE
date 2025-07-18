﻿using System.ComponentModel.DataAnnotations.Schema;

namespace nexle_api.Models
{
    [Table("token")]
    public class Token
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RefreshToken { get; set; } = null!;
        public string ExpiresIn { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
