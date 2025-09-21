using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace api.DTOs
{
    public class RegisterUserDto
    {
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ProfilePhotoFileId { get; set; } = string.Empty;
        public Point Location { get; set; } = null!;
        public string? Bio { get; set; }
    }
}