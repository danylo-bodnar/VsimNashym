using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class RegisterUserDto
    {
        public long TelegramId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }

        [MaxLength(200)]
        public string? Bio { get; set; }
        public string ProfilePhotoFileId { get; set; } = string.Empty;
    }
}