using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Connections;
using api.Enums;
using api.Interfaces;
using api.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
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

            var toUser = await _userService.GetUserByTelegramIdAsync(toTelegramId)
                  ?? throw new InvalidOperationException($"User {toTelegramId} not found");

            double? distanceKm = null;

            if (fromUser.LocationConsent &&
                toUser.LocationConsent &&
                fromUser.Location != null &&
                toUser.Location != null)
            {
                distanceKm = GeoUtils.CalculateDistanceKm(
                    fromUser.Location,
                    toUser.Location
                );
            }

            var photos = fromUser.ProfilePhotos
                    .OrderBy(p => p.SlotIndex)
                    .Take(3)
                    .ToList();

            if (photos.Any())
            {
                var media = photos.Select(p =>
                new InputMediaPhoto(p.Url)
                ).ToList<IAlbumInputMedia>();

                await _botMessenger.SendAlbumSafeAsync(
                    toTelegramId,
                    media
                );
            }

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    "✅ Accept",
                    $"accept:{result.Connection.Id}")
            }
            });

            var messageText = TelegramMessageBuilder.BuildUserIntro(
                  fromUser,
                  distanceKm
              );

            await _botMessenger.SendMessageSafeAsync(
                toTelegramId,
                messageText,
                keyboard
            );
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
