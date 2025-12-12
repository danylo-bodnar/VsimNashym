using api.Helpers;
using api.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace api.Services
{
    public class BotHandler
    {
        private readonly IBotMessenger _botMessenger;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public BotHandler(
             IBotMessenger botMessenger, IConnectionService connectionService, IMessageService messageService, IUserService userService)
        {
            _botMessenger = botMessenger;
            _connectionService = connectionService;
            _messageService = messageService;
            _userService = userService;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQueryAsync(update.CallbackQuery!);
                return;
            }

            if (update.Type != UpdateType.Message) return;

            var message = update.Message;
            if (message == null || message.From == null) return;

            long chatId = message.Chat.Id;
            long telegramId = message.From.Id;
            string? text = message.Text?.Trim();

            if (text == "/start")
            {
                await _userService.CreateSimpleUserAsync(telegramId, message.From.FirstName ?? "Unknown", message.From.LanguageCode ?? "en");
                return;
            }
        }


        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            long telegramId = callbackQuery.From.Id;
            string? data = callbackQuery.Data;

            if (string.IsNullOrEmpty(data))
                return;

            // Connections
            if (data.StartsWith("accept:") && int.TryParse(data.Split(':')[1], out int connectionId))
            {
                var connection = await _connectionService.AcceptConnectionAsync(connectionId);

                if (callbackQuery.Message != null)
                {
                    await _botMessenger.EditMessageReplyMarkupSafeAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        newText: "✅ Connection accepted!",
                        keyboard: null
                    );
                }

                if (connection != null)
                {
                    string contactLink = $"tg://user?id={telegramId}";

                    string contactInfo =
                        $"{callbackQuery.From.FirstName} accepted your hi! 🎉\n" +
                        $"You can now chat directly with {contactLink}";

                    await _botMessenger.SendMessageSafeAsync(
                        connection.FromTelegramId,
                        contactInfo
                    );
                }

                return;
            }

            // Location Consent 
            // if (data.StartsWith("locationConsent:"))
            // {
            //     long userTelegramId = long.Parse(data.Split(':')[1]);
            //
            //     await _userService.SaveLocationConsentAsync(userTelegramId);
            //
            //     string okText = "👍";
            //
            //     await _botMessenger.EditMessageReplyMarkupSafeAsync(
            //         chatId: callbackQuery.Message!.Chat.Id,
            //         messageId: callbackQuery.Message.MessageId,
            //         newText: okText,
            //         keyboard: null
            //     );
            //
            //     return;
            // }
        }
    }
}
