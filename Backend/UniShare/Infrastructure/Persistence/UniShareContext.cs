using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Users;

namespace UniShare.Infrastructure.Persistence;

public class UniShareContext(DbContextOptions<UniShareContext> options) : DbContext(options)
{
    // It will be made with a proper dto record
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map the User entity to a lowercase 'users' table to avoid Postgres quoted identifier issues
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            // map columns explicitly to snake_case to match common Postgres conventions
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
        
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("items");
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Condition).HasColumnName("condition");
            entity.Property(e => e.DailyRate).HasColumnName("daily_rate");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.IsAvailable).HasColumnName("is_available");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}