using api.DTOs.Connections;
using api.Models;

namespace api.Mappings
{
    public static class ConnectionMappers
    {
        public static Connection ToEntity(this CreateConnectionDto dto)
        {
            return new Connection
            {
                FromTelegramId = dto.FromTelegramId,
                ToTelegramId = dto.ToTelegramId,
                CreatedAt = DateTime.UtcNow,
                Status = ConnectionStatus.Pending
            };
        }
    }
}