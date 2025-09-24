using api.DTOs;
using api.Interfaces;
using api.Models;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace api.Services
{
    public class BotHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IBotConversationService _conversationService;
        private readonly IUserService _userService;
        private readonly ILogger<BotHandler> _logger;
        private readonly IBotMessenger _botMessenger;

        public BotHandler(
            ITelegramBotClient botClient,
            IBotConversationService conversationService,
            IUserService userService,
            ILogger<BotHandler> logger, IBotMessenger botMessenger)
        {
            _botClient = botClient;
            _conversationService = conversationService;
            _userService = userService;
            _logger = logger;
            _botMessenger = botMessenger;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type != UpdateType.Message) return;

            var message = update.Message;
            if (message == null || message.From == null) return;

            long chatId = message.Chat.Id;
            long telegramId = message.From.Id;
            string? text = message.Text?.Trim();

            var state = _conversationService.GetState(telegramId);

            try
            {
                // if (state.Step == ConversationStep.WaitingForPhoto && message.Photo != null && message.Photo.Length > 0)
                // {
                //     var photo = message.Photo.Last();
                //     string photoUrl = photo.FileId;

                //     if (string.IsNullOrEmpty(photoUrl))
                //     {
                //         await SendMessageSafeAsync(chatId, "Invalid photo. Please try sending again.");
                //         return;
                //     }

                //     state.TempProfilePhotoFileId = photoUrl;
                //     state.Step = ConversationStep.WaitingForLocation;
                //     _conversationService.SetState(telegramId, state);

                //     var keyboard = new ReplyKeyboardMarkup(new[]
                //  {
                //             KeyboardButton.WithRequestLocation("Share Location")
                //             })
                //     {
                //         ResizeKeyboard = true,
                //         OneTimeKeyboard = true
                //     };

                //     await SendMessageSafeAsync(chatId, "Please share your location.", keyboard);
                //     return;
                // }

                // if (string.IsNullOrEmpty(text) && state.Step != ConversationStep.WaitingForPhoto)
                // {
                //     await SendMessageSafeAsync(chatId, "Please send text messages or a photo when prompted.");
                //     return;
                // }

                if (text == "/register")
                {
                    if (await _userService.IsUserRegisteredAsync(telegramId))
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "You are already registered!");
                        return;
                    }
                    var newState = new UserConversationState { Step = ConversationStep.WaitingForDisplayName };
                    _conversationService.SetState(telegramId, newState);
                    await _botMessenger.SendMessageSafeAsync(chatId, "Welcome! Please send your display name.");
                    return;
                }

                if (state.Step == ConversationStep.WaitingForDisplayName)
                {
                    if (string.IsNullOrWhiteSpace(text) || text.Length > 50)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "Invalid display name. Try again (max 50 chars).");
                        return;
                    }

                    state.TempDisplayName = text;
                    state.Step = ConversationStep.WaitingForAge;
                    _conversationService.SetState(telegramId, state);


                    await _botMessenger.SendMessageSafeAsync(chatId, "Please enter your age.");
                    return;
                }

                if (state.Step == ConversationStep.WaitingForAge)
                {
                    if (string.IsNullOrWhiteSpace(text) || !int.TryParse(text, out int age) || age < 13 || age > 120)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "Please enter a valid age (between 13 and 120).");
                        return;
                    }

                    state.TempAge = age;
                    state.Step = ConversationStep.WaitingForPhoto;
                    _conversationService.SetState(telegramId, state);

                    await _botMessenger.SendMessageSafeAsync(chatId, "Please send a photo.");
                    return;
                }

                if (state.Step == ConversationStep.WaitingForPhoto)
                {
                    if (message.Photo == null || message.Photo.Length == 0)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "A photo is required to continue registration. Please send a valid photo.");
                        return;
                    }

                    var photo = message.Photo.Last();
                    string photoFileId = photo.FileId;

                    if (string.IsNullOrEmpty(photoFileId))
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "Invalid photo. Please try sending a valid photo.");
                        return;
                    }

                    state.TempProfilePhotoFileId = photoFileId;
                    state.Step = ConversationStep.WaitingForLocation;
                    _conversationService.SetState(telegramId, state);


                    var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                            KeyboardButton.WithRequestLocation("Share Location")
                            })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };

                    await _botMessenger.SendMessageSafeAsync(chatId, "Please share your location.", keyboard);
                    return;
                }
                if (state.Step == ConversationStep.WaitingForLocation)
                {
                    if (message.Location == null)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "Please share a valid location.");
                        return;
                    }

                    var point = new Point(message.Location.Longitude, message.Location.Latitude) { SRID = 4326 };

                    state.TempLocation = point;
                    state.Step = ConversationStep.WaitingForBio;

                    _conversationService.SetState(telegramId, state);
                    var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Skip") })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };

                    await _botMessenger.SendMessageSafeAsync(chatId, "Location received! Please write a short bio (or press 'Skip').", keyboard);
                    return;
                }

                if (state.Step == ConversationStep.WaitingForBio)
                {
                    string? bio = string.Equals(text, "skip", StringComparison.OrdinalIgnoreCase) ? null : text;

                    if (bio?.Length > 200)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "Bio too long. Try again (max 200 chars).");
                        return;
                    }

                    string displayName = state.TempDisplayName ?? throw new InvalidOperationException("Missing display name");
                    string photoFileId = state.TempProfilePhotoFileId ?? throw new InvalidOperationException("Missing photoFileId");
                    int age = state.TempAge ?? throw new InvalidOperationException("Missing age");
                    Point location = state.TempLocation ?? throw new InvalidOperationException("Missing location");

                    var dto = new RegisterUserDto
                    {
                        TelegramId = telegramId,
                        DisplayName = displayName,
                        Age = age,
                        ProfilePhotoFileId = photoFileId,
                        Location = location,
                        Bio = bio
                    };

                    var user = await _userService.RegisterUserAsync(dto);
                    if (user == null)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "You are already registered!");
                        _conversationService.Reset(telegramId);
                        return;
                    }

                    await _botMessenger.SendPhotoSafeAsync(
                            chatId,
                            user.ProfilePhotoFileId,
                            $"✅ Registration complete!\n" +
                            $"Display Name: {user.DisplayName}\n" +
                            $"Age: {user.Age}\n" +
                            $"Bio: {user.Bio ?? "None"}\n" +
                            $"Profile Photo: {(string.IsNullOrEmpty(user.ProfilePhotoFileId) ? "None" : "Added")}"
                        );

                    _conversationService.Reset(telegramId);
                    _logger.LogInformation("User registered: TelegramId {TelegramId}, Photo: {HasPhoto}", telegramId, !string.IsNullOrEmpty(state.TempProfilePhotoFileId));
                }
                else
                {
                    await _botMessenger.SendMessageSafeAsync(chatId, "Unknown command. Send /register to start.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling update for TelegramId {TelegramId}", telegramId);
                await _botMessenger.SendMessageSafeAsync(chatId, "Sorry, something went wrong. Try /register again.");
                _conversationService.Reset(telegramId);
            }
        }

    }
}