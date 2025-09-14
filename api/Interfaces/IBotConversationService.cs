using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IBotConversationService
    {
        UserConversationState GetState(long telegramId);
        void SetState(long telegramId, UserConversationState state);
        void Reset(long telegramId);
    }
}