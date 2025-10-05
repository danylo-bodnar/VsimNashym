using api.DTOs;
using api.DTOs.Users;
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
        public static UserDto ToUserDto(User user)
        {
            return new UserDto
            {
                TelegramId = user.TelegramId,
                DisplayName = user.DisplayName,
                Age = user.Age,
                Bio = user.Bio,
                Interests = user.Interests,
                LookingFor = user.LookingFor,
                Languages = user.Languages,
                CreatedAt = user.CreatedAt,
                ProfilePhotos = user.ProfilePhotos
                    .Select(p => new ProfilePhotoDto
                    {
                        Id = p.Id,
                        Url = p.Url,
                        MessageId = p.MessageId
                    })
                    .ToList(),
                Location = new LocationPointDto
                {
                    Latitude = user.Location.Y,
                    Longitude = user.Location.X
                }
            };
        }
    }
}