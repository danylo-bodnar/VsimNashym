using api.DTOs.Users;
using api.Models;
using NetTopologySuite.Geometries;

namespace api.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User userModel);
        Task SaveChangesAsync();
        Task<List<NearbyUserDto>> GetNearbyAsync(Point currentLocation, double radiusMeters, Guid currentUserId);
        Task<User?> GetByTelegramIdAsync(long telegramId);
        Task<bool> Exists(long telegramId);
        Task<List<User>> GetInactiveUsersOlderThanAsync(DateTime cutoff, CancellationToken token);
        Task SaveLocationConsentAsync(long telegramId);
    }
}
