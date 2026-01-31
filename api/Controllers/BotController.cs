using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace api.Controllers
{
    [ApiController]
    [Route("bot")]
    public class BotController : ControllerBase
    {
        private readonly BotHandler _botHandler;
        public BotController(BotHandler botHandler)
        {
            _botHandler = botHandler;
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] Update update)
        {
            await _botHandler.HandleUpdateAsync(update);
            return Ok();
        }
    }
}