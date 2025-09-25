using api.DTOs.ChatSessions;
using api.Models;

namespace api.Mappings
{
    public static class ChatSessionMappers
    {
        public static ChatSession ToEntity(this CreateChatSessionDto dto)
        {
            return new ChatSession
            {
                User1TelegramId = dto.User1TelegramId,
                User2TelegramId = dto.User2TelegramId,
                CreatedAt = DateTime.UtcNow,
            };
        }
    }
}