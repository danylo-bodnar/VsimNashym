using api.DTOs.Connections;
using api.Enums;
using api.Interfaces;
using api.Mappings;
using api.Models;
using api.Results;

namespace api.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly IConnectionRepository _connectionRepository;

        public ConnectionService(IConnectionRepository repository)
        {
            _connectionRepository = repository;
        }

        public async Task<ConnectionCreateResult> CreateConnectionAsync(CreateConnectionDto dto)
        {
            var existing = await _connectionRepository.GetAsync(
                dto.FromTelegramId,
                dto.ToTelegramId);

            if (existing != null)
            {
                return new ConnectionCreateResult
                {
                    Result = ConnectionResult.AlreadyExists,
                    Connection = existing
                };
            }

            if (await _connectionRepository.SentRecentlyAsync(
                dto.FromTelegramId,
                dto.ToTelegramId,
                TimeSpan.FromSeconds(30)))
            {
                return new ConnectionCreateResult
                {
                    Result = ConnectionResult.Cooldown
                };
            }

            var connection = await _connectionRepository.CreateAsync(dto.ToEntity());

            return new ConnectionCreateResult
            {
                Result = ConnectionResult.Created,
                Connection = connection
            };
        }

        public async Task<Connection> AcceptConnectionAsync(int connectionId)
        {
            return await _connectionRepository.AcceptAsync(connectionId);
        }

        public async Task<bool> ConnectionExistsAsync(long fromTelegramId, long toTelegramId)
        {
            return await _connectionRepository.ExistsAsync(fromTelegramId, toTelegramId);
        }
    }
}
