
using Telegram.Bot.Types.ReplyMarkups;

namespace api.Interfaces
{
    public interface IBotMessenger
    {
        Task SendMessageSafeAsync(long chatId, string text, ReplyMarkup? keyboard = null);
        Task SendPhotoSafeAsync(long chatId, string photoFileId, string? description = null);
        Task EditMessageReplyMarkupSafeAsync(long chatId, int messageId, InlineKeyboardMarkup? keyboard = null, string? newText = null);
    }
}