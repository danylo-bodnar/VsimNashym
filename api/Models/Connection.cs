namespace api.Models
{
    public class Connection
    {
        public int Id { get; set; }
        public long FromTelegramId { get; set; }
        public long ToTelegramId { get; set; }
        public ConnectionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ConnectionStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class ChatSession
    {
        public int Id { get; set; }
        public long User1TelegramId { get; set; }
        public long User2TelegramId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
