using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Models;

namespace api.Mappings
{
    public static class UserMappers
    {
        public static User ToEntity(this RegisterUserDto dto)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                TelegramId = dto.TelegramId,
                DisplayName = dto.DisplayName,
                Age = dto.Age,
                ProfilePhotoFileId = dto.ProfilePhotoFileId,
                Location = dto.Location,
                Bio = dto.Bio,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}