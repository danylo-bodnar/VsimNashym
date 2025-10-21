public class UpdateUserDto
{
    public long TelegramId { get; set; }
    public string? DisplayName { get; set; }
    public int? Age { get; set; }
    public IFormFile? Avatar { get; set; }
    public IFormFile[]? ProfilePhotos { get; set; }
    public List<int>? ProfilePhotoSlotIndices { get; set; } // <-- NEW
    public List<string>? ExistingPhotoMessageIds { get; set; }
    public string? Bio { get; set; }
    public List<string>? Interests { get; set; }
    public List<string>? LookingFor { get; set; }
    public List<string>? Languages { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
