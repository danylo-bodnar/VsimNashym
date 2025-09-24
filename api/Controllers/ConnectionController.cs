
using api.DTOs.Connections;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace api.Controllers
{
    [Route("[controller]")]
    public class ConnectionController : Controller
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect([FromBody] CreateConnectionDto dto)
        {
            if (await _connectionService.ConnectionExistsAsync(dto.FromTelegramId, dto.ToTelegramId))
                return BadRequest("Connection already exists.");

            var created = await _connectionService.CreateConnectionAsync(dto);
            return Ok(created);
        }
    }
}