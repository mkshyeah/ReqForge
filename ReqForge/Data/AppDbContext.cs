using System.IO;
using Microsoft.EntityFrameworkCore;
using ReqForge.Models;
using ReqForge.Models.DTOs;

namespace ReqForge.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RequestCollection> Collections => Set<RequestCollection>();
    public DbSet<SavedRequest> SavedRequests => Set<SavedRequest>();
    public DbSet<HeaderItemDto> RequestHeaders => Set<HeaderItemDto>();
    public DbSet<RequestEnvironmentDto> Environments => Set<RequestEnvironmentDto>();
    public DbSet<EnvironmentVariableDto> EnvironmentVariables => Set<EnvironmentVariableDto>();
    public DbSet<RequestHistoryItem> RequestHistory => Set<RequestHistoryItem>();

    private static readonly string DbPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reqforge.db");

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.UserName).IsUnique();
            e.HasMany(u => u.Collections)
                .WithOne()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(u => u.History)
                .WithOne()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequestCollection>(e =>
        {
            e.HasMany(c => c.Requests)
                .WithOne()
                .HasForeignKey(r => r.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SavedRequest>(e =>
        {
            e.HasMany(r => r.Headers)
                .WithOne()
                .HasForeignKey(h => h.SavedRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequestEnvironmentDto>(e =>
        {
            e.HasMany(env => env.Variables)
                .WithOne()
                .HasForeignKey(v => v.EnvironmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
