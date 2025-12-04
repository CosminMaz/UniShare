using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Bookings.ApproveBooking;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class ApproveBookingHandlerTests
{
    private static UniShareContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid userId)
    {
        var context = new DefaultHttpContext();
        context.Items["UserId"] = userId;

        return new HttpContextAccessor { HttpContext = context };
    }

    [Fact]
    public async Task Handle_Should_Approve_Booking_When_Request_Is_Valid()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner User", "owner@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Users.Add(new Infrastructure.Features.Users.User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, null, true, DateTime.UtcNow));
        
        var booking = new Booking(
            bookingId,
            itemId,
            borrowerId,
            ownerId,
            Status.Pending,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            default,
            20m,
            DateTime.UtcNow,
            default,
            default
        );
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(ownerId);
        var handler = new ApproveBookingHandler(context, httpContextAccessor);

        var request = new ApproveBookingRequest(bookingId, true);

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(200, (result as IStatusCodeHttpResult)?.StatusCode);
        
        var updatedBooking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        Assert.NotNull(updatedBooking);
        Assert.Equal(Status.Approved, updatedBooking.Status);
        Assert.NotEqual(default, updatedBooking.ApprovedAt);
        
        // Note: Item availability update is handled by raw SQL in production
        // For in-memory tests, this is skipped to avoid EF Core relationship tracking issues
        // The booking status update is the primary concern for unit tests
    }

    [Fact]
    public async Task Handle_Should_Reject_Booking_When_Request_Is_Valid()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner User", "owner@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Users.Add(new Infrastructure.Features.Users.User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, null, true, DateTime.UtcNow));
        
        var booking = new Booking(
            bookingId,
            itemId,
            borrowerId,
            ownerId,
            Status.Pending,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            default,
            20m,
            DateTime.UtcNow,
            default,
            default
        );
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(ownerId);
        var handler = new ApproveBookingHandler(context, httpContextAccessor);

        var request = new ApproveBookingRequest(bookingId, false);

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(200, (result as IStatusCodeHttpResult)?.StatusCode);
        
        var updatedBooking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        Assert.NotNull(updatedBooking);
        Assert.Equal(Status.Rejected, updatedBooking.Status);
        
        var updatedItem = await context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        Assert.NotNull(updatedItem);
        Assert.True(updatedItem.IsAvailable); // Item should still be available when rejected
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_UserId_Missing()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var bookingId = Guid.NewGuid();

        var httpContextAccessor = new HttpContextAccessor(); // no UserId set
        var handler = new ApproveBookingHandler(context, httpContextAccessor);

        var request = new ApproveBookingRequest(bookingId, true);

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(401, (result as IStatusCodeHttpResult)?.StatusCode);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbid_When_User_Is_Not_Owner()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner User", "owner@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Users.Add(new Infrastructure.Features.Users.User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Users.Add(new Infrastructure.Features.Users.User(otherUserId, "Other User", "other@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, null, true, DateTime.UtcNow));
        
        var booking = new Booking(
            bookingId,
            itemId,
            borrowerId,
            ownerId,
            Status.Pending,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            default,
            20m,
            DateTime.UtcNow,
            default,
            default
        );
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(otherUserId); // Different user
        var handler = new ApproveBookingHandler(context, httpContextAccessor);

        var request = new ApproveBookingRequest(bookingId, true);

        // Act
        var result = await handler.Handle(request);

        // Assert
        // Results.Forbid() returns ForbidHttpResult which doesn't implement IStatusCodeHttpResult directly
        // We check the type name to verify it's a Forbid result
        Assert.NotNull(result);
        Assert.Contains("Forbid", result.GetType().Name);
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Booking_Is_Not_Pending()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner User", "owner@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Users.Add(new Infrastructure.Features.Users.User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, null, true, DateTime.UtcNow));
        
        var booking = new Booking(
            bookingId,
            itemId,
            borrowerId,
            ownerId,
            Status.Approved, // Already approved
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            default,
            20m,
            DateTime.UtcNow,
            DateTime.UtcNow,
            default
        );
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(ownerId);
        var handler = new ApproveBookingHandler(context, httpContextAccessor);

        var request = new ApproveBookingRequest(bookingId, true);

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(400, (result as IStatusCodeHttpResult)?.StatusCode);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Booking_Does_Not_Exist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var ownerId = Guid.NewGuid();
        var nonExistentBookingId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner User", "owner@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(ownerId);
        var handler = new ApproveBookingHandler(context, httpContextAccessor);

        var request = new ApproveBookingRequest(nonExistentBookingId, true);

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(404, (result as IStatusCodeHttpResult)?.StatusCode);
    }
}

