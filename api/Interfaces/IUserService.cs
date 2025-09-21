using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Models;

namespace api.Interfaces
{
    public interface IUserService
    {
        Task<User?> RegisterUserAsync(RegisterUserDto dto);
        Task<IEnumerable<NearbyUserDto>> GetNearbyUsersAsync(Guid currentUserId, double lat, double lng, double radiusMeters);
        Task<bool> IsUserRegisteredAsync(long telegramId);
    }
}