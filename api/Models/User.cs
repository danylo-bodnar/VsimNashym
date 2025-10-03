
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;


namespace api.Models
{
    public class ProfilePhoto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Url { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; } = null!;
    }


    public class User
    {
        public Guid Id { get; set; }
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
        public List<ProfilePhoto> ProfilePhotos { get; set; } = new();
        public string? Bio { get; set; }
        public List<string> Interests { get; set; } = new();
        public List<string> LookingFor { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public Point Location { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}