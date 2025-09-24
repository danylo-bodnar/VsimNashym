using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using Telegram.Bot;

namespace api.Services
{
    public class MessageService : IMessageService
    {
        private readonly IBotMessenger _botMessenger;
        private readonly IUserRepository _userRepository;
        public MessageService(IUserRepository userRepository, ITelegramBotClient botClient, IBotMessenger botMessenger)
        {
            _userRepository = userRepository;
            _botMessenger = botMessenger;
        }

        async public Task SendHiAsync(long fromTelegramId, long toTelegramId)
        {
            var currentUser = await _userRepository.GetByTelegramIdAsync(fromTelegramId);

            await _botMessenger.SendMessageSafeAsync(toTelegramId, $"{currentUser.DisplayName} says hi 👋");
        }


    }
}