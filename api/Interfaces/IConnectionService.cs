using api.DTOs.Connections;
using api.Models;

namespace api.Interfaces
{
    public interface IConnectionService
    {
        Task<Connection> AcceptConnectionAsync(int connectionId);
        Task<Connection> CreateConnectionAsync(CreateConnectionDto dto);
        Task<bool> ConnectionExistsAsync(long fromTelegramId, long toTelegramId);
    }
}