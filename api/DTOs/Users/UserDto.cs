using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.DTOs.Users
{
    public class UserDto
    {
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Bio { get; set; }
        public List<string> Interests { get; set; } = new();
        public List<string> LookingFor { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public Avatar Avatar { get; set; } = null!;
        public List<ProfilePhotoDto> ProfilePhotos { get; set; } = new();
        public LocationPointDto Location { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool LocationConsent { get; set; } = false;
    }

    public class ProfilePhotoDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public int SlotIndex { get; set; }
    }

    public class LocationPointDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}