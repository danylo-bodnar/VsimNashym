using api.DTOs.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
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
                return StatusCode(500, "Token generation failed");

            return Ok(new { token });
        }

    }
}