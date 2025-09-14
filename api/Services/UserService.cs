using api.DTOs;
using api.Interfaces;
using api.Mappings;
using api.Models;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepositoy)
        {
            _userRepository = userRepositoy;
        }

        public async Task<User?> RegisterUserAsync(RegisterUserDto dto)
        {
            var exists = await _userRepository.Exists(dto.TelegramId);
            if (exists) return null;

            var userModel = dto.ToUserFromRegisterDto();

            var user = await _userRepository.CreateAsync(userModel);
            return user;
        }
        public async Task<bool> IsUserRegisteredAsync(long telegramId)
        {
            return await _userRepository.Exists(telegramId);
        }
    }
}