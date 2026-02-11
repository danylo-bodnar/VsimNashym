using api.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        public async Task<string> LoginWithTelegramAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);

            var claims = new List<Claim>
            {
                new Claim("telegram_id", telegramId.ToString()),
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
    }
}