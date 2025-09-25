using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTOs.ChatSessions;
using api.Interfaces;
using api.Mappings;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ChatSessionRepository : IChatSessionsRepository
    {
        private readonly ApplicationDbContext _db;

        public ChatSessionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        async public Task<ChatSession> CreateAsync(CreateChatSessionDto createChatSessionDto)
        {
            var chatSession = createChatSessionDto.ToEntity();

            await _db.ChatSessions.AddAsync(chatSession);
            await _db.SaveChangesAsync();

            return chatSession;
        }

        async public Task<ChatSession?> GetByUsersAsync(long user1TelegramId, long user2TelegramId)
        {
            return await _db.ChatSessions.FirstOrDefaultAsync(c =>
          (c.User1TelegramId == user1TelegramId && c.User2TelegramId == user2TelegramId) ||
          (c.User1TelegramId == user2TelegramId && c.User2TelegramId == user1TelegramId));
        }


        async public Task<ChatSession?> GetByUserAsync(long telegramId)
        {
            return await _db.ChatSessions
                   .FirstOrDefaultAsync(c => c.User1TelegramId == telegramId || c.User2TelegramId == telegramId);
        }
    }
}