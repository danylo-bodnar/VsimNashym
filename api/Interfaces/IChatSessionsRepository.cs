using api.DTOs.ChatSessions;
using api.Models;

namespace api.Interfaces
{
    public interface IChatSessionsRepository
    {
        Task<ChatSession> CreateAsync(CreateChatSessionDto createChatSessionDto);
        Task<ChatSession?> GetByUsersAsync(long user1TelegramId, long user2TelegramId);
        Task<ChatSession?> GetByUserAsync(long telegramId);
    }
}