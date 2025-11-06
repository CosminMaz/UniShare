using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Users;

namespace UniShare.Infrastructure.Persistence;

public class UniShareContext : DbContext
{
    public UniShareContext(DbContextOptions<UniShareContext> options) : base(options)
    {
    }

    // It will be made with a proper dto record
    public DbSet<User> Users { get; set; } = null!;

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
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}