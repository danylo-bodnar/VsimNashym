using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;


namespace api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
        public Avatar Avatar { get; set; } = null!;
        public List<ProfilePhoto> ProfilePhotos { get; set; } = new();
        public string? Bio { get; set; }
        public List<string> Interests { get; set; } = new();
        public List<string> LookingFor { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public Point Location { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    }

    public class Avatar
    {
        [Key]
        public string MessageId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    public class ProfilePhoto
    {
        [Key]
        public string MessageId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int SlotIndex { get; set; }
        public Guid UserId { get; set; }
    }

}
