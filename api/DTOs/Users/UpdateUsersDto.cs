public class UpdateUserDto
{
    public long TelegramId { get; set; }
    public string? DisplayName { get; set; }
    public int? Age { get; set; }
    public IFormFile? Avatar { get; set; }
    public IFormFile[]? ProfilePhotos { get; set; }
    public List<int>? ProfilePhotoSlotIndices { get; set; }
    public List<string>? ExistingPhotoMessageIds { get; set; }
    public string? Bio { get; set; }
    public List<string> Interests { get; set; } = new();
    public List<string> LookingFor { get; set; } = new();
    public List<string> Languages { get; set; } = new();
}
