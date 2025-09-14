using System.Collections.Concurrent;
using api.Interfaces;
using api.Models;

namespace api.Services
{
    public class BotConversationService : IBotConversationService
    {
        private readonly ConcurrentDictionary<long, UserConversationState> _userStates = new();
        public UserConversationState GetState(long telegramId)
        {
            _userStates.TryGetValue(telegramId, out var state);
            return state ?? new UserConversationState();
        }

        public void SetState(long telegramId, UserConversationState state)
        {
            _userStates[telegramId] = state;
        }

        public void Reset(long telegramId)
        {
            _userStates.TryRemove(telegramId, out _);
        }
    }
}

