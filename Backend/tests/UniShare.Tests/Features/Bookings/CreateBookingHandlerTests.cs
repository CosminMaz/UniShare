using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Bookings.CreateBooking;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Users;
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

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Item_Is_Unavailable()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var borrowerId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Users.Add(new User(ownerId, "Owner User", "owner@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, ownerId, "Item", "Desc", Category.Books, Condition.New, 15m, null, false, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = CreateHttpContextAccessor(borrowerId);
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var request = new CreateBookingRequest(itemId, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3));

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(400, (result as IStatusCodeHttpResult)?.StatusCode);
        Assert.Empty(context.Bookings);
    }

    [Fact]
    public async Task Handle_Should_Return_BadRequest_When_Owner_Does_Not_Exist()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var borrowerId = Guid.NewGuid();
        var missingOwnerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Users.Add(new User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, missingOwnerId, "Item", "Desc", Category.Books, Condition.New, 12m, null, true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = CreateHttpContextAccessor(borrowerId);
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var request = new CreateBookingRequest(itemId, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2));

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.Equal(400, (result as IStatusCodeHttpResult)?.StatusCode);
        Assert.Empty(context.Bookings);
    }

    [Fact]
    public async Task Handle_Should_Calculate_TotalPrice_From_Dates_And_Rate()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var borrowerId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var dailyRate = 15m;

        context.Users.Add(new User(ownerId, "Owner User", "owner@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, ownerId, "Item", "Desc", Category.Books, Condition.New, dailyRate, null, true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = CreateHttpContextAccessor(borrowerId);
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(4); // 4 days duration
        var request = new CreateBookingRequest(itemId, startDate, endDate);

        // Act
        var result = await handler.Handle(request);

        // Assert
        var created = Assert.IsType<Created<Booking>>(result);
        var booking = created.Value!;
        Assert.Equal(dailyRate * 4, booking.TotalPrice);
        Assert.Equal(startDate, booking.StartDate.Date);
        Assert.Equal(endDate, booking.EndDate.Date);
    }

    [Fact]
    public async Task Handle_Should_Return_ValidationProblem_When_Request_Invalid()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = new HttpContextAccessor(); // should not be used because validation fails
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var request = new CreateBookingRequest(Guid.Empty, DateTime.UtcNow.Date, DateTime.UtcNow.Date);

        // Act
        var result = await handler.Handle(request);

        // Assert
        var validationProblemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, validationProblemResult.StatusCode);
        Assert.Empty(context.Bookings);
    }

    [Fact]
    public async Task Handle_Should_Calculate_One_Day_When_Dates_Match()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var borrowerId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var dailyRate = 7m;

        context.Users.Add(new User(ownerId, "Owner User", "owner@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, ownerId, "Item", "Desc", Category.Books, Condition.New, dailyRate, null, true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = CreateHttpContextAccessor(borrowerId);
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var date = DateTime.UtcNow.Date;
        var request = new CreateBookingRequest(itemId, date, date);

        // Act
        var result = await handler.Handle(request);

        // Assert
        var created = Assert.IsType<Created<Booking>>(result);
        var booking = created.Value!;
        Assert.Equal(dailyRate * 1, booking.TotalPrice);
        Assert.Equal(date, booking.StartDate.Date);
        Assert.Equal(date, booking.EndDate.Date);
    }

    [Fact]
    public async Task Handle_Should_Set_TotalPrice_To_Zero_When_DailyRate_Null()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var borrowerId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Users.Add(new User(ownerId, "Owner User", "owner@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(borrowerId, "Borrower User", "borrower@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, ownerId, "Item", "Desc", Category.Books, Condition.New, null, null, true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var validator = new InlineValidator<CreateBookingRequest>();
        validator.RuleFor(x => x.ItemId).NotEmpty();

        var httpContextAccessor = CreateHttpContextAccessor(borrowerId);
        var handler = new CreateBookingHandler(context, validator, httpContextAccessor);

        var request = new CreateBookingRequest(itemId, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(2));

        // Act
        var result = await handler.Handle(request);

        // Assert
        var created = Assert.IsType<Created<Booking>>(result);
        var booking = created.Value!;
        Assert.Equal(0m, booking.TotalPrice);
    }
}

