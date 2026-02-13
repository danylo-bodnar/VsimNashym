using api.DTOs.Auth;
using api.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;
        private readonly string _botToken;

        public AuthService(ITokenService tokenService, IUserRepository userRepository, ILogger<AuthService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _botToken = configuration["TELEGRAM_BOT_TOKEN"]
                ?? throw new InvalidOperationException("Telegram Bot Token is missing in configuration");
        }

        public async Task<string?> LoginWithTelegramAsync(string initData)
        {
            if (!ValidateTelegramInitData(initData, _botToken))
                return null;

            // Parse the telegram ID directly
            var data = HttpUtility.ParseQueryString(initData);
            var userJson = data["user"];

            if (string.IsNullOrEmpty(userJson))
                return null;

            // Parse just the ID using JsonDocument (no class needed)
            using var doc = JsonDocument.Parse(userJson);
            var telegramId = doc.RootElement.GetProperty("id").GetInt64();

            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            var claims = new List<Claim>
    {
        new Claim("telegram_id", telegramId.ToString())
    };

            if (user != null)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.DisplayName));
                claims.Add(new Claim("is_registered", "true"));
            }
            else
            {
                claims.Add(new Claim("is_registered", "false"));
            }

            return _tokenService.GenerateToken(claims);
        }

        public static bool ValidateTelegramInitData(string initData, string botToken)
        {
            // Parse the initData query string
            var data = HttpUtility.ParseQueryString(initData);
            var dataDict = new SortedDictionary<string, string>(
                data.AllKeys.ToDictionary(x => x!, x => data[x]!), StringComparer.Ordinal
            );

            // Get the hash parameter and remove it from the dictionary
            if (!dataDict.ContainsKey("hash"))
                return false;
            var receivedHash = dataDict["hash"];
            dataDict.Remove("hash");

            // Create the data-check-string
            var dataCheckString = string.Join(
                '\n',
                dataDict.Select(x => $"{x.Key}={x.Value}")
            );

            // Generate the secret key: HMAC-SHA-256 of the bot token using "WebAppData" as the key
            byte[] secretKeyBytes;
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData")))
            {
                secretKeyBytes = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(botToken));
            }

            // Calculate the data hash: HMAC-SHA-256 of the data-check-string using the secret key
            byte[] calculatedHashBytes;
            using (var hmacsha256 = new HMACSHA256(secretKeyBytes))
            {
                calculatedHashBytes = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
            }

            // Convert the calculated hash to a hex string
            var calculatedHash = BitConverter.ToString(calculatedHashBytes).Replace("-", "").ToLower();

            // Compare the hashes
            return receivedHash == calculatedHash;
        }

    }
}