using api.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;

        public AuthService(ITokenService tokenService, IUserRepository userRepository, ILogger<AuthService> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<string?> LoginWithTelegramAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null)
            {
                _logger.LogWarning("Telegram login failed: {TelegramId} not registered", telegramId);
                return null;
            }

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.DisplayName),
            new Claim("telegram_id", user.TelegramId.ToString())
           };

            var token = _tokenService.GenerateToken(claims);
            _logger.LogInformation("Telegram login successful: {TelegramId}", telegramId);

            return token;
        }
    }
}