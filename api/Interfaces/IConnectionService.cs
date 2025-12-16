using api.DTOs.Connections;
using api.Models;
using api.Results;

namespace api.Interfaces
{
    public interface IConnectionService
    {
        Task<ConnectionCreateResult> CreateConnectionAsync(CreateConnectionDto dto);
        Task<Connection> AcceptConnectionAsync(int connectionId);
        Task<bool> ConnectionExistsAsync(long fromTelegramId, long toTelegramId);
    }
}
