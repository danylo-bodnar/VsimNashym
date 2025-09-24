using Telegram.Bot.Types.ReplyMarkups;

namespace api.Interfaces
{
    public interface IBotMessenger
    {
        Task SendMessageSafeAsync(long chatId, string text, ReplyKeyboardMarkup? keyboard = null);
        Task SendPhotoSafeAsync(long chatId, string photoFileId, string? description = null);
    }
}