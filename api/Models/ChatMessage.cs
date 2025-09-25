
namespace api.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }
        public long FromTelegramId { get; set; }
        public long ToTelegramId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ChatSession? ChatSession { get; set; }
    }
}
