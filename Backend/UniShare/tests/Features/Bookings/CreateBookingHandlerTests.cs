using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Bookings.CreateBooking;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class CreateBookingHandlerTests
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
    public async Task Handle_Should_Create_Booking_When_Request_Is_Valid()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var borrowerId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner User", "owner@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Users.Add(new Infrastructure.Features.Users.User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, null, true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = CreateHttpContextAccessor(borrowerId);
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var request = new CreateBookingRequest(itemId, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3));

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(201, (result as IStatusCodeHttpResult)?.StatusCode);
        Assert.Single(context.Bookings);
        var booking = await context.Bookings.FirstAsync();
        Assert.Equal(Status.Pending, booking.Status);
        Assert.Equal(borrowerId, booking.BorrowerId);
        Assert.Equal(ownerId, booking.OwnerId);
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_UserId_Missing()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var itemId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner User", "owner@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, null, true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = new HttpContextAccessor(); // no UserId set
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var request = new CreateBookingRequest(itemId, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3));

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(401, (result as IStatusCodeHttpResult)?.StatusCode);
        Assert.Empty(context.Bookings);
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Item_Does_Not_Exist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var borrowerId = Guid.NewGuid();

        context.Users.Add(new Infrastructure.Features.Users.User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = CreateHttpContextAccessor(borrowerId);
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var request = new CreateBookingRequest(Guid.NewGuid(), DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3));

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(400, (result as IStatusCodeHttpResult)?.StatusCode);
        Assert.Empty(context.Bookings);
    }
}


