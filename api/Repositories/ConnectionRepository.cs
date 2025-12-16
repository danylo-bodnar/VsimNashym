using api.Data;
using api.DTOs.Connections;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ConnectionRepository : IConnectionRepository
    {
        private ApplicationDbContext _db;

        public ConnectionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Connection> CreateAsync(Connection connectionModel)
        {
            await _db.Connections.AddAsync(connectionModel);
            await _db.SaveChangesAsync();

            return connectionModel;
        }

        async public Task<Connection> AcceptAsync(int connectionId)
        {
            var connection = await _db.Connections.FirstOrDefaultAsync(c => c.Id == connectionId);

            if (connection == null)
            {
                throw new InvalidOperationException($"Connection with Id {connectionId} not found.");
            }

            connection.Status = ConnectionStatus.Accepted;

            await _db.SaveChangesAsync();

            return connection;

        }

        async public Task<bool> ExistsAsync(long fromTelegramId, long toTelegramId)
        {
            return await _db.Connections.AnyAsync(u => u.FromTelegramId == fromTelegramId && u.ToTelegramId == toTelegramId);
        }

        public async Task<bool> SentRecentlyAsync(long fromId, long toId, TimeSpan cooldown)
        {
            return await _db.Connections
                .AnyAsync(c =>
                    c.FromTelegramId == fromId &&
                    c.ToTelegramId == toId &&
                    c.CreatedAt > DateTime.UtcNow.Subtract(cooldown)
                );
        }

        public async Task<Connection?> GetAsync(long fromTelegramId, long toTelegramId)
        {
            return await _db.Connections
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.FromTelegramId == fromTelegramId &&
                    c.ToTelegramId == toTelegramId);
        }
    }
}
