using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Items.Delete;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;
using Xunit;

namespace UniShare.tests.Features.Items;

public class DeleteItemHandlerTests : IDisposable
{
    private readonly UniShareContext _context;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly DeleteItemHandler _handler;

    public DeleteItemHandlerTests()
    {
        Log.Info("Setting up DeleteItemHandlerTests...");
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_DeleteItem")
            .Options;
        _context = new UniShareContext(options);
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new DeleteItemHandler(_context, _httpContextAccessorMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidOwner_DeletesItem()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var item = new Item(Guid.NewGuid(), ownerId, "Title", "Desc", Category.Books, Condition.New, 10, null, true, DateTime.UtcNow);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = ownerId;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(item.Id);

        // Assert
        Assert.IsType<NoContent>(result);
        var itemInDb = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
        Assert.Null(itemInDb);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ReturnsNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = ownerId;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task Handle_NotOwner_ReturnsForbid()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var item = new Item(Guid.NewGuid(), ownerId, "Title", "Desc", Category.Books, Condition.New, 10, null, true, DateTime.UtcNow);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = otherUserId;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(item.Id);

        // Assert
        Assert.IsType<ForbidHttpResult>(result);
        var itemStillThere = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.Id);
        Assert.NotNull(itemStillThere);
    }

    [Fact]
    public async Task Handle_HasBookings_ReturnsConflict()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var booking = new Booking(
            Guid.NewGuid(),
            itemId,
            Guid.NewGuid(), // borrower
            ownerId,
            Status.Pending,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(1),
            default,
            0,
            DateTime.UtcNow,
            default,
            default);

        var item = new Item(itemId, ownerId, "Title", "Desc", Category.Books, Condition.New, 10, null, true, DateTime.UtcNow);
        _context.Items.Add(item);
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = ownerId;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Conflict", result.GetType().Name);
        var itemStillThere = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        Assert.NotNull(itemStillThere);
    }

    [Fact]
    public async Task Handle_HttpContextNull_ReturnsUnauthorized()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = await _handler.Handle(Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Handle_UserIdMissing_ReturnsUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}

