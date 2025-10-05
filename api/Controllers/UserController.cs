using System.Security.Claims;
using api.DTOs;
using api.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace api.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IValidator<RegisterUserDto> _validator;

        public UserController(IUserService userService, IValidator<RegisterUserDto> validator)
        {
            _userService = userService;
            _validator = validator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto dto)
        {
            await _validator.ValidateAsync(dto);

            var user = await _userService.RegisterUserAsync(dto);

            if (user == null)
                return Conflict(new { message = "User with this Telegram ID already exists." });

            return Ok(new
            {
                telegramId = user.TelegramId,
                displayName = user.DisplayName,
                age = user.Age,
                profilePhotos = user.ProfilePhotos,
                bio = user.Bio,
                interests = user.Interests,
                lookingFor = user.LookingFor,
                languages = user.Languages,
                location = new { latitude = user.Location.Y, longitude = user.Location.X },
                createdAt = user.CreatedAt
            });
        }

        [Authorize]
        [HttpGet("{telegramId:long}")]
        public async Task<IActionResult> GetUserByTelegramId(long telegramId)
        {
            var user = await _userService.GetUserByTelegramIdAsync(telegramId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [Authorize]
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyUsers(double lat, double lng, double radiusMeters)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var currentUserId))
                return Unauthorized();

            var nearbyUsers = await _userService.FindNearbyUsersAsync(currentUserId, lat, lng, radiusMeters);
            return Ok(nearbyUsers);
        }

    }
}