using api.Data;
using api.Interfaces;
using api.Models;

namespace api.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {

        private readonly ApplicationDbContext _db;

        public ChatMessageRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ChatMessage> CreateAsync(ChatMessage message)
        {
            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();
            return message;
        }
    }
}