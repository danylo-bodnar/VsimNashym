using api.DTOs;
using api.Models;
using NetTopologySuite.Geometries;

namespace api.Mappings
{
    public static class UserMappers
    {
        public static User ToEntity(this RegisterUserDto dto, List<(string url, string messageId)>? uploadedPhotos = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                TelegramId = dto.TelegramId,
                DisplayName = dto.DisplayName,
                Age = dto.Age,
                ProfilePhotos = uploadedPhotos?.Select(p => new ProfilePhoto
                {
                    Url = p.url,
                    MessageId = p.messageId
                }).ToList() ?? new List<ProfilePhoto>(),
                Location = new Point(dto.Longitude, dto.Latitude) { SRID = 4326 },
                Bio = dto.Bio,
                Interests = dto.Interests ?? new List<string>(),
                LookingFor = dto.LookingFor ?? new List<string>(),
                Languages = dto.Languages ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };
        }

    }
}