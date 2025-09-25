using api.Models;

namespace api.Interfaces
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage> CreateAsync(ChatMessage message);
    }
}