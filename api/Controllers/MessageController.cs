using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [HttpPost("hi")]
        public async Task<IActionResult> SendHi([FromBody] HiDto dto)
        {
            await _messageService.SendHiAsync(User.GetTelegramId(), dto.To);
            return Ok();
        }

    }
}