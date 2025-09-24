using api.DTOs.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("telegram-login")]
        public async Task<IActionResult> TelegramLogin([FromBody] TelegramLoginRequest request)
        {
            var token = await _authService.LoginWithTelegramAsync(request.TelegramId);

            if (token == null)
                return Unauthorized("User not registered");

            return Ok(new { token });
        }
    }
}