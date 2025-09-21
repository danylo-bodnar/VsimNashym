using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Models;
using NetTopologySuite.Geometries;

namespace api.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User userModel);
        Task<List<NearbyUserDto>> GetNearbyUsersAsync(Point currentLocation, double radiusMeters, Guid currentUserId);
        Task<User?> GetUserByTelegramIdAsync(long telegramId);
        Task<bool> Exists(long telegramId);
    }
}