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

            modelBuilder.Entity<Avatar>(entity =>
            {
                entity.HasKey(a => a.MessageId);

                entity.Property(a => a.Url)
                      .IsRequired();

                entity.HasOne<User>()
                      .WithOne(u => u.Avatar)
                      .HasForeignKey<Avatar>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Connection>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.HasIndex(h => new { h.FromTelegramId, h.ToTelegramId }).IsUnique();
                entity.Property(h => h.CreatedAt).HasDefaultValueSql("NOW()");
            });

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (tableName != null)
                    entity.SetTableName(tableName.ToLower());

                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnName();
                    if (columnName != null)
                        property.SetColumnName(columnName.ToLower());
                }
            }

            base.OnModelCreating(modelBuilder);

        }
    }
}