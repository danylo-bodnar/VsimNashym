using api.DTOs;
using api.Interfaces;
using api.Models;
using NetTopologySuite.Geometries;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace api.Services
{
    public class BotHandler
    {
        private readonly IBotConversationService _conversationService;
        private readonly IUserService _userService;
        private readonly ILogger<BotHandler> _logger;
        private readonly IBotMessenger _botMessenger;
        private readonly IConnectionService _connectionService;

        public BotHandler(
            IBotConversationService conversationService,
            IUserService userService,
            ILogger<BotHandler> logger, IBotMessenger botMessenger, IConnectionService connectionService)
        {
            _conversationService = conversationService;
            _userService = userService;
            _logger = logger;
            _botMessenger = botMessenger;
            _connectionService = connectionService;
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

            var state = _conversationService.GetState(telegramId);

            try
            {
                // --- Start registration ---
                if (text == "/register")
                {
                    if (await _userService.IsUserRegisteredAsync(telegramId))
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "You are already registered!");
                        return;
                    }

                    _conversationService.SetState(telegramId, new UserConversationState
                    {
                        Step = ConversationStep.WaitingForDisplayName
                    });

                    await _botMessenger.SendMessageSafeAsync(chatId, "Welcome! Please send your display name.");
                    return;
                }

                // --- Step: Display Name ---
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

                // --- Step: Age ---
                if (state.Step == ConversationStep.WaitingForAge)
                {
                    if (!int.TryParse(text, out int age) || age < 13 || age > 120)
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

                // --- Step: Photo ---
                if (state.Step == ConversationStep.WaitingForPhoto)
                {
                    if (message.Photo == null || message.Photo.Length == 0)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "A photo is required. Please send a valid photo.");
                        return;
                    }

                    state.TempProfilePhotoFileId = message.Photo.Last().FileId;
                    state.Step = ConversationStep.WaitingForLocation;
                    _conversationService.SetState(telegramId, state);

                    var keyboard = new ReplyKeyboardMarkup(new[] { KeyboardButton.WithRequestLocation("Share Location") })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };
                    await _botMessenger.SendMessageSafeAsync(chatId, "Please share your location.", keyboard);
                    return;
                }

                // --- Step: Location ---
                if (state.Step == ConversationStep.WaitingForLocation)
                {
                    if (message.Location == null)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, "Please share a valid location.");
                        return;
                    }

                    state.TempLocation = new Point(message.Location.Longitude, message.Location.Latitude) { SRID = 4326 };
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

                // --- Step: Bio & Final Registration ---
                if (state.Step == ConversationStep.WaitingForBio)
                {
                    string? bio = string.Equals(text, "skip", StringComparison.OrdinalIgnoreCase) ? null : text;

                    // Build DTO
                    var dto = new RegisterUserDto
                    {
                        TelegramId = telegramId,
                        DisplayName = state.TempDisplayName!,
                        Age = state.TempAge!.Value,
                        ProfilePhotoFileId = state.TempProfilePhotoFileId!,
                        Location = state.TempLocation!,
                        Bio = bio
                    };

                    var validator = new RegisterUserValidator();
                    var validationResult = validator.Validate(dto);
                    if (!validationResult.IsValid)
                    {
                        await _botMessenger.SendMessageSafeAsync(chatId, $"Validation error: {validationResult.Errors.First().ErrorMessage}");
                        return;
                    }

                    // Register user
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
                        $"✅ Registration complete!\nDisplay Name: {user.DisplayName}\nAge: {user.Age}\nBio: {user.Bio ?? "None"}", new ReplyKeyboardRemove()
                    );

                    _conversationService.Reset(telegramId);
                    _logger.LogInformation("User registered: TelegramId {TelegramId}, Photo: {HasPhoto}", telegramId, !string.IsNullOrEmpty(state.TempProfilePhotoFileId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling update for TelegramId {TelegramId}", telegramId);
                await _botMessenger.SendMessageSafeAsync(chatId, "Sorry, something went wrong. Try /register again.");
                _conversationService.Reset(telegramId);
            }
        }
        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            long telegramId = callbackQuery.From.Id;
            string? data = callbackQuery.Data;

            if (string.IsNullOrEmpty(data))
                return;

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
            }
        }

    }
}
