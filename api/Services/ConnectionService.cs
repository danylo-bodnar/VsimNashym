using api.DTOs.Connections;
using api.Interfaces;
using api.Mappings;
using api.Models;

namespace api.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly IConnectionRepository _connectionRepository;

        public ConnectionService(IConnectionRepository repository)
        {
            _connectionRepository = repository;
        }

        public async Task<Connection> CreateConnectionAsync(CreateConnectionDto dto)
        {
            var connectionModel = dto.ToEntity();
            var connection = await _connectionRepository.CreateAsync(connectionModel);

            return connection;
        }

        async public Task<Connection> AcceptConnectionAsync(int connectionId)
        {
            var connection = await _connectionRepository.AcceptAsync(connectionId);

            return connection;
        }

        public async Task<bool> ConnectionExistsAsync(long fromTelegramId, long toTelegramId)
        {
            return await _connectionRepository.ExistsAsync(fromTelegramId, toTelegramId);
        }

    }
}