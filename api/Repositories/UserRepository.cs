using api.Data;
using api.DTOs;
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
                telegramid,
                displayname,
                ST_Y(location) AS latitude,
                ST_X(location) AS longitude
            FROM users
            WHERE id <> @currentUserId
              AND ST_DWithin(
                  location::geography,
                  ST_SetSRID(ST_MakePoint(@lng, @lat), 4326)::geography,
                  @radiusMeters
              )
            ORDER BY ST_Distance(
                location::geography,
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
                    Location = new LocationPoint
                    {
                        Latitude = reader.GetDouble(reader.GetOrdinal("latitude")),
                        Longitude = reader.GetDouble(reader.GetOrdinal("longitude"))
                    }
                });
            }

            return results;
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            return await _db.Users
                .Include(u => u.ProfilePhotos)
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }

        public async Task<bool> Exists(long telegramId)
        {
            return await _db.Users.AnyAsync(u => u.TelegramId == telegramId);
        }

    }
}