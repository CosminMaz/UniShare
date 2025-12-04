using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Reviews;
using UniShare.Infrastructure.Features.Users;

namespace UniShare.Infrastructure.Persistence;

public class UniShareContext(DbContextOptions<UniShareContext> options) : DbContext(options)
{
    // It will be made with a proper dto record
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;

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
            entity.Property(e => e.Categ)
                .HasColumnName("category")
                .HasConversion<string>();
            entity.Property(e => e.Cond)
                .HasColumnName("condition")
                .HasConversion<string>();
            entity.Property(e => e.DailyRate).HasColumnName("daily_rate");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.IsAvailable).HasColumnName("is_available");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
        
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("bookings");

            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("uuid");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.BorrowerId).HasColumnName("borrower_id");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>(); // saved as text to match SQL CHECK enum

            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ActualReturnDate).HasColumnName("actual_return_date");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.RequestedAt).HasColumnName("requested_at");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");

            // Foreign Keys
            entity.HasOne<Item>()
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("uuid");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.RevType)
                .HasColumnName("review_type")
                .HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}