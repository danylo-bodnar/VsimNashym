using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ProfilePhotoFileId { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}