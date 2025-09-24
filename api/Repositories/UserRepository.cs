using api.Data;
using api.DTOs;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        async public Task<User> CreateAsync(User userModel)
        {
            await _db.Users.AddAsync(userModel);
            await _db.SaveChangesAsync();

            return userModel;
        }

        async public Task<List<NearbyUserDto>> GetNearbyAsync(Point currentLocation, double radiusMeters, Guid currentUserId)
        {
            return await _db.Users
                            .Where(u => u.Location != null && u.Id != currentUserId)
                            .Where(u => u.Location.Distance(currentLocation) <= radiusMeters)
                            .Select(u => new NearbyUserDto
                            {
                                TelegramId = u.TelegramId,
                                DisplayName = u.DisplayName,
                                ProfilePhotoFileId = u.ProfilePhotoFileId,
                                Location = new LocationPoint
                                {
                                    Latitude = u.Location.Y,
                                    Longitude = u.Location.X
                                }
                            })
                            .ToListAsync();
        }


        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task<bool> Exists(long telegramId)
        {
            return await _db.Users.AnyAsync(u => u.TelegramId == telegramId);
        }

    }
}