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
            var exists = await _userRepository.Exists(dto.TelegramId);
            if (exists) throw new ApplicationException($"User with TelegramId {dto.TelegramId} already exists.");

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

            (string url, string messageId) avatar = await _fileStorageService.UploadProfilePhotoAsync(dto.Avatar);

            var userModel = dto.ToEntity(avatar, uploadedPhotos);

            var user = await _userRepository.CreateAsync(userModel);

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

            if (dto.ExistingPhotoMessageIds != null)
            {
                var photosToRemove = user.ProfilePhotos
                    .Where(p => !dto.ExistingPhotoMessageIds.Contains(p.MessageId))
                    .ToList();

                foreach (var photo in photosToRemove)
                {
                    await _fileStorageService.DeleteProfilePhotoAsync(photo.MessageId);
                    user.ProfilePhotos.Remove(photo);
                }
            }

            if (dto.ProfilePhotos != null && dto.ProfilePhotoSlotIndices != null)
            {
                for (int i = 0; i < dto.ProfilePhotos.Length; i++)
                {
                    var file = dto.ProfilePhotos[i];
                    var slotIndex = dto.ProfilePhotoSlotIndices[i];

                    if (slotIndex < 0 || slotIndex > 2) continue;

                    var uploaded = await _fileStorageService.UploadProfilePhotoAsync(file);

                    var existing = user.ProfilePhotos.FirstOrDefault(p => p.SlotIndex == slotIndex);
                    if (existing != null)
                    {
                        await _fileStorageService.DeleteProfilePhotoAsync(existing.MessageId);
                        existing.Url = uploaded.url;
                        existing.MessageId = uploaded.messageId;
                    }
                    else
                    {
                        user.ProfilePhotos.Add(new ProfilePhoto
                        {
                            Url = uploaded.url,
                            MessageId = uploaded.messageId,
                            SlotIndex = slotIndex,
                            UserId = user.Id
                        });
                    }
                }
            }

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

            await _userRepository.UpdateAsync(user);
            return user;
        }


        public async Task<IEnumerable<NearbyUserDto>> FindNearbyUsersAsync(Guid currentUserId, double lat, double lng, double radiusMeters)
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
    }
}