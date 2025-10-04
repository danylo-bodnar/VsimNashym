using api.Interfaces;
using Discord;
using Discord.WebSocket;

namespace api.Services
{
    public class DiscordFileStorageService : IFileStorageService
    {
        private readonly string _botToken;
        private readonly ulong _channelId;
        private readonly DiscordSocketClient _client;

        public DiscordFileStorageService(string botToken, ulong channelId)
        {
            _botToken = botToken;
            _channelId = channelId;
            _client = new DiscordSocketClient();

            _client.LoginAsync(TokenType.Bot, _botToken).GetAwaiter().GetResult();
            _client.StartAsync().GetAwaiter().GetResult();
        }
        public async Task<(string url, string messageId)> UploadProfilePhotoAsync(IFormFile file)
        {
            if (file.Length == 0) throw new ArgumentException("File is empty");

            var channel = _client.GetChannel(_channelId) as IMessageChannel;
            if (channel == null) throw new Exception("Discord channel not found");

            using var stream = file.OpenReadStream();
            var message = await channel.SendFileAsync(stream, file.FileName);

            var attachment = message.Attachments.FirstOrDefault();
            if (attachment == null) throw new Exception("Failed to upload file to Discord");

            return (attachment.Url, message.Id.ToString());
        }

        public async Task DeleteProfilePhotoAsync(string messageId)
        {
            var channel = _client.GetChannel(_channelId) as IMessageChannel;
            if (channel == null) throw new Exception("Discord channel not found");

            if (ulong.TryParse(messageId, out var discordMessageId))
            {
                var message = await channel.GetMessageAsync(discordMessageId);
                if (message != null) await message.DeleteAsync();
            }
        }


    }
}