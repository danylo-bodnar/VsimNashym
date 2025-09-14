using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{

    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("ping")]
        public IActionResult Ping([FromBody] object body)
        {
            Console.WriteLine("Ping received!");
            Console.WriteLine(body?.ToString() ?? "No body");

            return Ok(new { status = "success", message = "Ping received!" });
        }
    }

}