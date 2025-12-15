using api.DTOs;
using api.DTOs.Users;
using api.Models;
using NetTopologySuite.Geometries;

namespace api.Mappings
{
    public static class UserMappers
    {
        public static User ToEntity(this RegisterUserDto dto, (string url, string messageId) uploadedAvatar, List<(string url, string messageId, int slotIndex)>? uploadedPhotos = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                TelegramId = dto.TelegramId,
                DisplayName = dto.DisplayName,
                Age = dto.Age,
                Avatar =
                    new Avatar
                    {
                        Url = uploadedAvatar.url,
                        MessageId = uploadedAvatar.messageId
                    }
            ,
                ProfilePhotos = uploadedPhotos?.Select(p => new ProfilePhoto
                {
                    Url = p.url,
                    MessageId = p.messageId,
                    SlotIndex = p.slotIndex,

                }).ToList() ?? new List<ProfilePhoto>(),
                Bio = dto.Bio,
                Interests = dto.Interests ?? new List<string>(),
                LookingFor = dto.LookingFor ?? new List<string>(),
                Languages = dto.Languages ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static UserDto ToUserDto(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

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

                Avatar = user.Avatar,

                ProfilePhotos = user.ProfilePhotos?
                    .Select(p => new ProfilePhotoDto
                    {
                        Url = p.Url,
                        MessageId = p.MessageId,
                        SlotIndex = p.SlotIndex
                    })
                    .ToList() ?? new List<ProfilePhotoDto>(),

                Location = user.Location == null
                    ? null
                    : new LocationPointDto
                    {
                        Latitude = user.Location.Y,
                        Longitude = user.Location.X
                    },

                LocationConsent = user.LocationConsent
            };
        }

        public static void UpdateEntity(this UpdateUserDto dto, User user, List<(string url, string messageId)>? uploadedPhotos = null)
        {
            if (dto.DisplayName != null) user.DisplayName = dto.DisplayName;
            if (dto.Age.HasValue) user.Age = dto.Age.Value;
            if (dto.Bio != null) user.Bio = dto.Bio;
            if (dto.Interests != null) user.Interests = dto.Interests;
            if (dto.LookingFor != null) user.LookingFor = dto.LookingFor;
            if (dto.Languages != null) user.Languages = dto.Languages;

            if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            {
                user.Location = new Point(dto.Longitude.Value, dto.Latitude.Value) { SRID = 4326 };
            }

            // Filter existing photos
            if (dto.ExistingPhotoMessageIds != null && dto.ExistingPhotoMessageIds.Any())
            {
                user.ProfilePhotos = user.ProfilePhotos
                    .Where(p => dto.ExistingPhotoMessageIds.Contains(p.MessageId))
                    .ToList();
            }

            // Add new uploaded photos
            if (uploadedPhotos != null && uploadedPhotos.Any())
            {
                foreach (var p in uploadedPhotos)
                {
                    // Make sure no duplicate
                    if (!user.ProfilePhotos.Any(x => x.MessageId == p.messageId))
                    {
                        user.ProfilePhotos.Add(new ProfilePhoto
                        {
                            Url = p.url,
                            MessageId = p.messageId
                        });
                    }
                }
            }
        }

    }
}
