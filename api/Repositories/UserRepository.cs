using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        async public Task<User> CreateAsync(User userModel)
        {
            await _db.Users.AddAsync(userModel);
            await _db.SaveChangesAsync();

            return userModel;
        }

        public async Task<bool> Exists(long telegramId)
        {
            return await _db.Users.AnyAsync(u => u.TelegramId == telegramId);
        }
    }
}