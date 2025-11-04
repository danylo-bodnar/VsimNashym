using api.Data;
using api.DTOs.Users;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Data;

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

        public async Task<List<NearbyUserDto>> GetNearbyAsync(Point currentLocation, double radiusMeters, Guid currentUserId)
        {
            var sql = @"
            SELECT 
                u.telegramid,
                u.displayname,
                ST_Y(u.location) AS latitude,
                ST_X(u.location) AS longitude,
                a.url AS avatar_url,
                u.lastactiveat
            FROM users u
            LEFT JOIN avatar a 
                ON a.userid = u.id
            WHERE u.id <> @currentUserId
            AND ST_DWithin(
                u.location::geography,
                ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography,
                @radiusMeters
            )
            ORDER BY ST_Distance(
                u.location::geography,
                ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography
            );
            ";

            var results = new List<NearbyUserDto>();

            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@currentUserId";
            p1.Value = currentUserId;
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@lng";
            p2.Value = currentLocation.X;
            cmd.Parameters.Add(p2);

            var p3 = cmd.CreateParameter();
            p3.ParameterName = "@lat";
            p3.Value = currentLocation.Y;
            cmd.Parameters.Add(p3);

            var p4 = cmd.CreateParameter();
            p4.ParameterName = "@radiusMeters";
            p4.Value = radiusMeters;
            cmd.Parameters.Add(p4);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new NearbyUserDto
                {
                    TelegramId = reader.GetInt64(reader.GetOrdinal("telegramid")),
                    DisplayName = reader.GetString(reader.GetOrdinal("displayname")),
                    AvatarUrl = reader.IsDBNull(reader.GetOrdinal("avatar_url"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("avatar_url")),
                    Location = new LocationPoint
                    {
                        Latitude = reader.GetDouble(reader.GetOrdinal("latitude")),
                        Longitude = reader.GetDouble(reader.GetOrdinal("longitude"))
                    },
                    LastActive = reader.GetDateTime(reader.GetOrdinal("lastactiveat")).GetLastActiveLabel()
                });
            }

            return results;
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            return await _db.Users
                .Include(u => u.ProfilePhotos)
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task<bool> Exists(long telegramId)
        {
            return await _db.Users.AnyAsync(u => u.TelegramId == telegramId);
        }

        public async Task<User> UpdateAsync(User user)
        {
            await _db.SaveChangesAsync();
            return user;
        }
    }
}
