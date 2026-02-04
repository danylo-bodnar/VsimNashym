using api.DTOs;
using api.DTOs.Users;

namespace Api.IntegrationTests.Helpers
{
    public static class MultipartHelper
    {
        public static MultipartFormDataContent ToMultipart(this RegisterUserDto dto)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(dto.TelegramId.ToString()), nameof(dto.TelegramId) },
                { new StringContent(dto.DisplayName ?? ""), nameof(dto.DisplayName) },
                { new StringContent(dto.Age.ToString()), nameof(dto.Age) },
                { new StringContent(dto.Bio ?? ""), nameof(dto.Bio) },
            };

            if (dto.Interests != null)
            {
                foreach (var interest in dto.Interests)
                {
                    content.Add(new StringContent(interest), nameof(dto.Interests));
                }
            }

            if (dto.Languages != null)
            {
                foreach (var language in dto.Languages)
                {
                    content.Add(new StringContent(language), nameof(dto.Languages));
                }
            }

            if (dto.LookingFor != null)
            {
                foreach (var item in dto.LookingFor)
                {
                    content.Add(new StringContent(item), nameof(dto.LookingFor));
                }
            }

            if (dto.Avatar != null)
            {
                var avatarStream = new MemoryStream();
                dto.Avatar.CopyTo(avatarStream);
                avatarStream.Position = 0;
                var avatarContent = new StreamContent(avatarStream);
                avatarContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(
                    avatarContent,
                    nameof(dto.Avatar),
                    dto.Avatar.FileName
                );
            }

            if (dto.ProfilePhotos != null && dto.ProfilePhotos.Length > 0)
            {
                for (int i = 0; i < dto.ProfilePhotos.Length; i++)
                {
                    var stream = new MemoryStream();
                    dto.ProfilePhotos[i].CopyTo(stream);
                    stream.Position = 0;
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                    content.Add(fileContent, nameof(dto.ProfilePhotos), $"photo{i}.png");
                }
            }

            if (dto.ProfilePhotoSlotIndices != null && dto.ProfilePhotoSlotIndices.Count > 0)
            {
                foreach (var index in dto.ProfilePhotoSlotIndices)
                {
                    content.Add(new StringContent(index.ToString()), nameof(dto.ProfilePhotoSlotIndices));
                }
            }

            return content;
        }

        public static MultipartFormDataContent ToMultipart(this UpdateUserDto dto)
        {
            var content = new MultipartFormDataContent();

            if (dto.TelegramId != default)
                content.Add(new StringContent(dto.TelegramId.ToString()), nameof(dto.TelegramId));

            if (!string.IsNullOrEmpty(dto.DisplayName))
                content.Add(new StringContent(dto.DisplayName), nameof(dto.DisplayName));

            if (dto.Age.HasValue)
                content.Add(new StringContent(dto.Age.Value.ToString()), nameof(dto.Age));

            if (!string.IsNullOrEmpty(dto.Bio))
                content.Add(new StringContent(dto.Bio), nameof(dto.Bio));

            if (dto.Interests != null)
            {
                foreach (var interest in dto.Interests)
                    content.Add(new StringContent(interest), nameof(dto.Interests));
            }

            if (dto.Languages != null)
            {
                foreach (var language in dto.Languages)
                    content.Add(new StringContent(language), nameof(dto.Languages));
            }

            if (dto.LookingFor != null)
            {
                foreach (var item in dto.LookingFor)
                    content.Add(new StringContent(item), nameof(dto.LookingFor));
            }

            if (dto.Avatar != null)
            {
                var avatarStream = new MemoryStream();
                dto.Avatar.CopyTo(avatarStream);
                avatarStream.Position = 0;
                var avatarContent = new StreamContent(avatarStream);
                avatarContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                content.Add(
                               avatarContent,
                               nameof(dto.Avatar),
                               dto.Avatar.FileName
                           );
            }

            if (dto.ProfilePhotos != null && dto.ProfilePhotos.Length > 0)
            {
                for (int i = 0; i < dto.ProfilePhotos.Length; i++)
                {
                    var stream = new MemoryStream();
                    dto.ProfilePhotos[i].CopyTo(stream);
                    stream.Position = 0;
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                    content.Add(fileContent, nameof(dto.ProfilePhotos), $"photo{i}.png");
                }
            }

            if (dto.ProfilePhotoSlotIndices != null && dto.ProfilePhotoSlotIndices.Count > 0)
            {
                foreach (var index in dto.ProfilePhotoSlotIndices)
                    content.Add(new StringContent(index.ToString()), nameof(dto.ProfilePhotoSlotIndices));
            }

            if (dto.ExistingPhotoMessageIds != null)
            {
                foreach (var id in dto.ExistingPhotoMessageIds)
                    content.Add(new StringContent(id.ToString()), nameof(dto.ExistingPhotoMessageIds));
            }

            return content;
        }
    }
}
