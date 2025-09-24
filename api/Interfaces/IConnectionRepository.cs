using api.DTOs.Connections;
using api.Models;

namespace api.Interfaces
{
    public interface IConnectionRepository
    {
        Task<Connection> CreateAsync(Connection connectionModel);
        Task<Connection> AcceptAsync(int connectionId);
        Task<bool> ExistsAsync(long fromTelegramId, long toTelegramId);
    }
}