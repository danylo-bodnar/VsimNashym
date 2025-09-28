using api.DTOs;
using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("[controller]")]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [Authorize]
        [HttpPost("hi")]
        public async Task<IActionResult> SendHi([FromBody] HiDto dto)
        {
            if (User == null)
            {
                throw new UnauthorizedAccessException("User is null (not authenticated).");
            }

            var telegramId = User?.GetTelegramId();

            if (telegramId == null)
            {
                return Unauthorized("User is not authenticated or missing TelegramId.");
            }

            await _messageService.SendHiAsync(User!.GetTelegramId(), dto.To);

            return Ok();
        }

    }
}