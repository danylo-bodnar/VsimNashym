namespace api.DTOs
{
    public class RegisterUserDto
    {
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
        public IFormFile Avatar { get; set; } = null!;
        public IFormFile[] ProfilePhotos { get; set; } = null!;
        public List<int> ProfilePhotoSlotIndices { get; set; } = null!;
        public string? Bio { get; set; }
        public List<string> Interests { get; set; } = new();
        public List<string> LookingFor { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}