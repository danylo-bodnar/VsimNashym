using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace api.Services
{
    public class BotMessenger : IBotMessenger
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<BotMessenger> _logger;

        public BotMessenger(ITelegramBotClient botClient, ILogger<BotMessenger> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task SendMessageSafeAsync(long chatId, string text, ReplyMarkup? keyboard = null)
        {
            try
            {
                if (keyboard != null)
                {
                    await _botClient.SendMessage(chatId, text, replyMarkup: keyboard);
                }
                else
                {
                    await _botClient.SendMessage(chatId, text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to chat {ChatId}: {Text}", chatId, text);
            }
        }

        public async Task SendPhotoSafeAsync(long chatId, string ProfilePhotoFileId, string? description = null)
        {
            try
            {
                await _botClient.SendPhoto(chatId, ProfilePhotoFileId, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send a photo to chat {ChatId}, with photoUrl {Photo}", chatId, ProfilePhotoFileId);
            }
        }

        public async Task EditMessageReplyMarkupSafeAsync(long chatId, int messageId, InlineKeyboardMarkup? keyboard = null, string? newText = null)
        {
            try
            {
                if (newText != null)
                {
                    await _botClient.EditMessageText(
                        chatId: chatId,
                        messageId: messageId,
                        text: newText,
                        replyMarkup: keyboard
                    );
                }
                else
                {
                    await _botClient.EditMessageReplyMarkup(
                        chatId: chatId,
                        messageId: messageId,
                        replyMarkup: keyboard
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit message {MessageId} in chat {ChatId}", messageId, chatId);
            }
        }
    }
}