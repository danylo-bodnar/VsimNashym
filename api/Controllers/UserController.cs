using System.Security.Claims;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace api.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;


        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyUsers(double lat, double lng, double radiusMeters)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var currentUserId))
                return Unauthorized();

            var nearbyUsers = await _userService.GetNearbyUsersAsync(currentUserId, lat, lng, radiusMeters);
            return Ok(nearbyUsers);
        }

    }
}