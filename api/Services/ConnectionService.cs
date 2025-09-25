using api.DTOs.ChatSessions;
using api.DTOs.Connections;
using api.Interfaces;
using api.Mappings;
using api.Models;

namespace api.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly IConnectionRepository _connectionRepository;
        private readonly IChatSessionsRepository _chatSessionsRepository;

        public ConnectionService(IConnectionRepository repository, IChatSessionsRepository chatSessionsRepository)
        {
            _connectionRepository = repository;
            _chatSessionsRepository = chatSessionsRepository;
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

            await _chatSessionsRepository.CreateAsync(new CreateChatSessionDto
            {
                User1TelegramId = connection.FromTelegramId,
                User2TelegramId = connection.ToTelegramId
            });

            return connection;
        }

        public async Task<bool> ConnectionExistsAsync(long fromTelegramId, long toTelegramId)
        {
            return await _connectionRepository.ExistsAsync(fromTelegramId, toTelegramId);
        }

    }
}
