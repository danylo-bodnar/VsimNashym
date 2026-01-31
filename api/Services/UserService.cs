using api.DTOs;
using api.DTOs.Users;
using api.Interfaces;
using api.Mappings;
using api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using NetTopologySuite.Geometries;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;

        public UserService(IUserRepository userRepositoy, IFileStorageService fileStorageService)
        {
            _userRepository = userRepositoy;
            _fileStorageService = fileStorageService;
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto dto)
        {
            var user = await _userRepository.GetByTelegramIdAsync(dto.TelegramId);

            var avatarUpload = await _fileStorageService.UploadProfilePhotoAsync(dto.Avatar);

            var uploadedPhotos = new List<(string url, string messageId, int slotIndex)>();
            if (dto.ProfilePhotos != null && dto.ProfilePhotos.Any())
            {
                for (int i = 0; i < dto.ProfilePhotos.Length; i++)
                {
                    var file = dto.ProfilePhotos[i];
                    var slotIndex = dto.ProfilePhotoSlotIndices[i];

                    var uploaded = await _fileStorageService.UploadProfilePhotoAsync(file);
                    uploadedPhotos.Add((uploaded.url, uploaded.messageId, slotIndex));
                }
            }

            if (user == null)
            {
                var userModel = dto.ToEntity(avatarUpload, uploadedPhotos);
                user = await _userRepository.CreateAsync(userModel);
            }
            else
            {
                user.DisplayName = dto.DisplayName;
                user.Age = dto.Age;
                user.Bio = dto.Bio;
                user.Languages = dto.Languages;
                user.Interests = dto.Interests;
                user.LookingFor = dto.LookingFor;

                user.Avatar = new Avatar
                {
                    Url = avatarUpload.url,
                    MessageId = avatarUpload.messageId,
                    UserId = user.Id
                };

                user.ProfilePhotos = uploadedPhotos
                    .Select(p => new ProfilePhoto
                    {
                        Url = p.url,
                        MessageId = p.messageId,
                        SlotIndex = p.slotIndex
                    }).ToList();

                user.HasFullProfile = true;

                await _userRepository.SaveChangesAsync();
            }

            return user;
        }

        public async Task<User> UpdateUserAsync(long telegramId, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null)
                throw new KeyNotFoundException($"User with TelegramId {telegramId} not found.");

            // --- BASIC FIELDS ---
            if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                user.DisplayName = dto.DisplayName;

            if (dto.Age.HasValue)
                user.Age = dto.Age.Value;

            if (!string.IsNullOrWhiteSpace(dto.Bio))
                user.Bio = dto.Bio;

            if (dto.Interests != null)
                user.Interests = dto.Interests;

            if (dto.LookingFor != null)
                user.LookingFor = dto.LookingFor;

            if (dto.Languages != null)
                user.Languages = dto.Languages;

            if (dto.Latitude.HasValue && dto.Longitude.HasValue)
                user.Location = new Point(dto.Longitude.Value, dto.Latitude.Value) { SRID = 4326 };

            // --- PHOTOS ---
            var finalPhotos = new ProfilePhoto[3];

            foreach (var photo in user.ProfilePhotos)
            {
                if (photo.SlotIndex >= 0 && photo.SlotIndex < 3)
                    finalPhotos[photo.SlotIndex] = photo;
            }

            var keepMessageIds = dto.ExistingPhotoMessageIds ?? new List<string>();

            for (int i = 0; i < finalPhotos.Length; i++)
            {
                var photo = finalPhotos[i];
                if (photo != null && !keepMessageIds.Contains(photo.MessageId))
                {
                    await _fileStorageService.DeleteProfilePhotoAsync(photo.MessageId);
                    finalPhotos[i] = null;
                }
            }

            if (dto.ProfilePhotos != null && dto.ProfilePhotoSlotIndices != null)
            {
                for (int i = 0; i < dto.ProfilePhotos.Length; i++)
                {
                    var file = dto.ProfilePhotos[i];
                    var slot = dto.ProfilePhotoSlotIndices[i];

                    if (slot < 0 || slot > 2) continue;

                    var uploaded = await _fileStorageService.UploadProfilePhotoAsync(file);

                    if (finalPhotos[slot] != null)
                        await _fileStorageService.DeleteProfilePhotoAsync(finalPhotos[slot].MessageId);

                    finalPhotos[slot] = new ProfilePhoto
                    {
                        Url = uploaded.url,
                        MessageId = uploaded.messageId,
                        SlotIndex = slot,
                        UserId = user.Id
                    };
                }
            }

            user.ProfilePhotos = finalPhotos
                .Where(p => p != null)
                .ToList()!;

            // --- AVATAR ---
            if (dto.Avatar != null)
            {
                if (user.Avatar != null)
                {
                    await _fileStorageService.DeleteProfilePhotoAsync(user.Avatar.MessageId);
                }

                var uploadedAvatar = await _fileStorageService.UploadProfilePhotoAsync(dto.Avatar);
                user.Avatar = new Avatar
                {
                    Url = uploadedAvatar.url,
                    MessageId = uploadedAvatar.messageId,
                    UserId = user.Id,
                };
            }

            await _userRepository.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<NearbyUserDto>> FindNearbyUsersAsync(long currentUserId, double lat, double lng, double radiusMeters)
        {
            var currentLocation = new Point(lng, lat) { SRID = 4326 };
            return await _userRepository.GetNearbyAsync(currentLocation, radiusMeters, currentUserId);
        }

        public async Task<bool> IsUserRegisteredAsync(long telegramId)
        {
            return await _userRepository.Exists(telegramId);
        }

        public async Task<UserDto?> GetUserByTelegramIdAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (user == null)
            {
                return null;
            }

            return UserMappers.ToUserDto(user);
        }

        public async Task<UserDto?> UpdateUserLocationAsync(long telegramId, UpdateUserLocationDto dto)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with TelegramId {telegramId} not found.");
            }

            var newLocation = new Point(dto.Longitude, dto.Latitude) { SRID = 4326 };

            user.Location = newLocation;

            await _userRepository.SaveChangesAsync();

            return UserMappers.ToUserDto(user);
        }

        public async Task MarkUserActiveAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null) return;

            user.LastActiveAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();
        }

        public async Task SaveLocationConsentAsync(long telegramId)
        {
            var user = await _userRepository.Exists(telegramId);

            if (!user) { throw new Exception($"User with TelegramId {telegramId} not found."); }

            await _userRepository.SaveLocationConsentAsync(telegramId);
        }

        public async Task<User> CreateSimpleUserAsync(long telegramId, string firstName, string langCode)
        {
            var exists = await _userRepository.Exists(telegramId);

            if (exists)
            {
                return (await _userRepository.GetByTelegramIdAsync(telegramId))!;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = firstName,
                TelegramId = telegramId,
                LanguageCode = langCode,
                HasFullProfile = false,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            return user;
        }
    }
}
