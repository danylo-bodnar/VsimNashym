namespace api.DTOs
{
    public class NearbyUserDto
    {
        public long TelegramId { get; set; }
        public string DisplayName { get; set; } = null!;
        public LocationPoint Location { get; set; } = null!;
    }

    public class LocationPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}