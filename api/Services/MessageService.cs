using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Connections;
using api.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace api.Services
{
    public class MessageService : IMessageService
    {
        private readonly IBotMessenger _botMessenger;
        private readonly IUserRepository _userRepository;
        private readonly IConnectionService _connectionService;

        public MessageService(IUserRepository userRepository, IConnectionService connectionService, IBotMessenger botMessenger)
        {
            _userRepository = userRepository;
            _botMessenger = botMessenger;
            _connectionService = connectionService;
        }

        async public Task SendHiAsync(long fromTelegramId, long toTelegramId)
        {

            // TODO: add spam prevention

            var connection = await _connectionService.CreateConnectionAsync(new CreateConnectionDto
            {
                FromTelegramId = fromTelegramId,
                ToTelegramId = toTelegramId
            });

            var fromUser = await _userRepository.GetByTelegramIdAsync(fromTelegramId);

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
            InlineKeyboardButton.WithCallbackData("✅ Accept", $"accept:{connection.Id}")
            }
            });

            await _botMessenger.SendMessageSafeAsync(toTelegramId, $"{fromUser.DisplayName} says hi 👋", keyboard);
        }
    }
}
