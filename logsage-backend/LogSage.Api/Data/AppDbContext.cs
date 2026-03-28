using LogSage.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<ErrorGroupEntity> ErrorGroups => Set<ErrorGroupEntity>();
    public DbSet<UsageTracking> UsageTracking => Set<UsageTracking>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e => {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Plan).HasDefaultValue("free");
        });
        b.Entity<RefreshToken>(e => {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Session>(e => {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.User)
             .WithMany(u => u.Sessions)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });
        b.Entity<ErrorGroupEntity>(e => {
            e.HasKey(eg => eg.Id);
            e.HasOne(eg => eg.Session)
             .WithMany(s => s.ErrorGroups)
             .HasForeignKey(eg => eg.SessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<UsageTracking>(e => {
            e.HasKey(u => u.Id);
            e.HasIndex(u => new { u.Identifier, u.Date }).IsUnique();
        });
    }
}
