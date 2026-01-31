
using api.DTOs.Connections;
using api.Enums;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect([FromBody] CreateConnectionDto dto)
        {
            var result = await _connectionService.CreateConnectionAsync(dto);

            return result.Result switch
            {
                ConnectionResult.Created => Ok(),
                ConnectionResult.AlreadyExists => Ok("Already connected"),
                ConnectionResult.Cooldown => StatusCode(429, "Please wait before sending another request."),
                _ => BadRequest()
            };
        }
    }
}
