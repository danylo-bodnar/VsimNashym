
namespace api.Interfaces
{
    public interface IChatMessageService
    {
        Task<bool> SendDirectMessageAsync(long fromTelegramId, long toTelegramId, string text);
    }
}