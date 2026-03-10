using LIAhub.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LIAhub.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserTechStack> UserTechStacks => Set<UserTechStack>();
    public DbSet<CachedJob> CachedJobs => Set<CachedJob>();
    public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<NotificationSetting> NotificationSettings => Set<NotificationSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) // Fluent API configurations
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.TechStacks)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.SavedJobs)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Applications)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.NotificationSetting)
            .WithOne(n => n.User)
            .HasForeignKey<NotificationSetting>(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CachedJob>()
            .Property(j => j.TechTags)
            .HasColumnType("text[]");
    }
}