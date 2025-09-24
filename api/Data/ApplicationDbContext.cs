using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.TelegramId).IsUnique();
                entity.Property(u => u.DisplayName).IsRequired();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(u => u.Location).HasColumnType("geometry(Point, 4326)");
            });

            modelBuilder.Entity<Connection>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.HasIndex(h => new { h.FromTelegramId, h.ToTelegramId }).IsUnique();
                entity.Property(h => h.CreatedAt).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => new { c.User1TelegramId, c.User2TelegramId }).IsUnique();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
            });
        }
    }
}