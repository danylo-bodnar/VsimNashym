using api.DTOs;
using api.DTOs.Users;
using api.Models;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegisterUserDto dto);
        Task<User> UpdateUserAsync(long telegramId, UpdateUserDto dto);
        Task<UserDto?> GetUserByTelegramIdAsync(long telegramId);
        Task<UserDto?> UpdateUserLocationAsync(long telegramId, UpdateUserLocationDto dto);
        Task<IEnumerable<NearbyUserDto>> FindNearbyUsersAsync(long currentUserId, double lat, double lng, double radiusMeters);
        Task<bool> IsUserRegisteredAsync(long telegramId);
        Task MarkUserActiveAsync(long telegramId);
        Task SaveLocationConsentAsync(long telegramId);
        Task<User> CreateSimpleUserAsync(long telegramId, string firstName, string langCode);
    }
}
