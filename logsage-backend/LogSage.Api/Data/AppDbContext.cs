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
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<ParseSession> ParseSessions => Set<ParseSession>();

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
        b.Entity<Subscription>(e => {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.ExternalSubscriptionId);
            e.HasIndex(s => new { s.UserId, s.Provider });
            e.HasOne(s => s.User)
             .WithMany()
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<ParseSession>(e => {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.SessionToken);
            e.Property(p => p.SessionToken).HasMaxLength(64);
            e.Property(p => p.DetectedFormat).HasMaxLength(50);
            e.Property(p => p.InputSample).HasMaxLength(500);
            // Store Metadata as JSONB for efficient querying in PostgreSQL
            e.Property(p => p.Metadata).HasColumnType("jsonb");
        });
    }
}
