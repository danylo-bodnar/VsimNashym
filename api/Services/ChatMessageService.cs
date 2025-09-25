using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;

namespace api.Services
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly IBotMessenger _botMessenger;
        private readonly IChatSessionsRepository _chatSessionsRepository;
        private readonly IChatMessageRepository _chatMessageRepository;

        public ChatMessageService(IBotMessenger botMessenger, IChatSessionsRepository chatSessionsRepository,
            IChatMessageRepository chatMessageRepository)
        {
            _botMessenger = botMessenger;
            _chatMessageRepository = chatMessageRepository;
            _chatSessionsRepository = chatSessionsRepository;
        }

        async public Task<bool> SendDirectMessageAsync(long fromTelegramId, long toTelegramId, string text)
        {
            var session = await _chatSessionsRepository.GetByUsersAsync(fromTelegramId, toTelegramId);
            if (session == null) return false;

            var chatMessage = new ChatMessage
            {
                ChatSessionId = session.Id,
                FromTelegramId = fromTelegramId,
                ToTelegramId = toTelegramId,
                Text = text
            };

            await _chatMessageRepository.CreateAsync(chatMessage);

            await _botMessenger.SendMessageSafeAsync(toTelegramId, text);
            return true;
        }
    }
}