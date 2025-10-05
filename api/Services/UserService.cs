using api.DTOs;
using api.DTOs.Users;
using api.Interfaces;
using api.Mappings;
using api.Models;
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

        public async Task<User?> RegisterUserAsync(RegisterUserDto dto)
        {
            var exists = await _userRepository.Exists(dto.TelegramId);
            if (exists) return null;

            var uploadedPhotos = new List<(string url, string messageId)>();
            if (dto.ProfilePhotos != null && dto.ProfilePhotos.Any())
            {
                foreach (var file in dto.ProfilePhotos)
                {
                    var uploaded = await _fileStorageService.UploadProfilePhotoAsync(file);
                    uploadedPhotos.Add(uploaded);
                }
            }

            var userModel = dto.ToEntity(uploadedPhotos);

            var user = await _userRepository.CreateAsync(userModel);

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