using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Bookings.GetAll;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class GetAllBookingsHandlerTests
{
    private static UniShareContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    [Fact]
    public async Task Handle_Returns_All_Bookings()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllBookings.Handler(context);

        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Users.Add(new User(ownerId, "Owner", "owner@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(borrowerId, "Borrower", "borrower@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, ownerId, "Item", "Desc", Category.Books, Condition.New, 10m, null, true, DateTime.UtcNow));

        context.Bookings.Add(new Booking(Guid.NewGuid(), itemId, borrowerId, ownerId, Status.Pending, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), default, 10m, DateTime.UtcNow, default, default));
        context.Bookings.Add(new Booking(Guid.NewGuid(), itemId, borrowerId, ownerId, Status.Approved, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2), default, 20m, DateTime.UtcNow, DateTime.UtcNow, default));
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetAllBookings.Query(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, b => b.Status == Status.Pending);
        Assert.Contains(result, b => b.Status == Status.Approved);
    }

    [Fact]
    public async Task Handle_When_No_Bookings_Returns_Empty()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllBookings.Handler(context);

        // Act
        var result = await handler.Handle(new GetAllBookings.Query(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Returns_Bookings_With_All_Statuses()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllBookings.Handler(context);

        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Users.Add(new User(ownerId, "Owner", "owner@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(borrowerId, "Borrower", "borrower@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, ownerId, "Item", "Desc", Category.Books, Condition.New, 10m, null, true, DateTime.UtcNow));

        context.Bookings.Add(new Booking(Guid.NewGuid(), itemId, borrowerId, ownerId, Status.Pending, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), default, 10m, DateTime.UtcNow, default, default));
        context.Bookings.Add(new Booking(Guid.NewGuid(), itemId, borrowerId, ownerId, Status.Approved, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2), default, 20m, DateTime.UtcNow, DateTime.UtcNow, default));
        context.Bookings.Add(new Booking(Guid.NewGuid(), itemId, borrowerId, ownerId, Status.Rejected, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(3), default, 30m, DateTime.UtcNow, default, default));
        context.Bookings.Add(new Booking(Guid.NewGuid(), itemId, borrowerId, ownerId, Status.Completed, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(4), DateTime.UtcNow.Date.AddDays(4), 40m, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow));
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetAllBookings.Query(), CancellationToken.None);

        // Assert
        Assert.Equal(4, result.Count());
        Assert.Contains(result, b => b.Status == Status.Pending);
        Assert.Contains(result, b => b.Status == Status.Approved);
        Assert.Contains(result, b => b.Status == Status.Rejected);
        Assert.Contains(result, b => b.Status == Status.Completed);
    }
}

