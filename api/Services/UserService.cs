using api.DTOs;
using api.Interfaces;
using api.Mappings;
using api.Models;
using NetTopologySuite.Geometries;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepositoy)
        {
            _userRepository = userRepositoy;
        }

        public async Task<User?> RegisterUserAsync(RegisterUserDto dto)
        {
            var exists = await _userRepository.Exists(dto.TelegramId);
            if (exists) return null;

            var userModel = dto.ToUserFromRegisterDto();

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
    }
}