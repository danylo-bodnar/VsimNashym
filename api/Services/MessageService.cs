using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Connections;
using api.Enums;
using api.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace api.Services
{
    public class MessageService : IMessageService
    {
        private readonly IBotMessenger _botMessenger;
        private readonly IUserService _userService;
        private readonly IConnectionService _connectionService;

        public MessageService(IUserService userService, IConnectionService connectionService, IBotMessenger botMessenger)
        {
            _userService = userService;
            _botMessenger = botMessenger;
            _connectionService = connectionService;
        }

        public async Task SendHiAsync(long fromTelegramId, long toTelegramId)
        {
            var result = await _connectionService.CreateConnectionAsync(
                new CreateConnectionDto
                {
                    FromTelegramId = fromTelegramId,
                    ToTelegramId = toTelegramId
                });

            if (result.Result == ConnectionResult.Cooldown)
                throw new InvalidOperationException("Cooldown active");

            if (result.Connection == null)
                throw new InvalidOperationException("Connection missing");

            var fromUser = await _userService.GetUserByTelegramIdAsync(fromTelegramId)
                ?? throw new InvalidOperationException($"User {fromTelegramId} not found");

            var keyboard = new InlineKeyboardMarkup(new[]
            {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(
                "✅ Accept",
                $"accept:{result.Connection.Id}")
        }
    });

            await _botMessenger.SendMessageSafeAsync(
                toTelegramId,
                $"{fromUser.DisplayName} says hi 👋",
                keyboard);
        }

        // async public Task SendLocationConsent(long chatId, long telegramId, string langCode)
        // {
        //     string consentText = ConsentTexts.GetConsentText(langCode);
        //
        //     var keyboard = new InlineKeyboardMarkup(new[]
        //            {
        //     new[]
        //     {
        //     InlineKeyboardButton.WithCallbackData("✅", $"locationConsent:{telegramId}")
        //     }
        //     });
        //
        //     await _botMessenger.SendMessageSafeAsync(chatId, consentText, keyboard);
        // }
    }
}
