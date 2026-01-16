using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Bookings.CompleteBooking;
using UniShare.Infrastructure.Persistence;
using UniShare.RealTime;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class CompleteBookingHandlerTests
{
    private static UniShareContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid? userId = null)
    {
        var context = new DefaultHttpContext();
        if (userId.HasValue)
        {
            context.Items["UserId"] = userId.Value;
        }

        return new HttpContextAccessor { HttpContext = context };
    }

    private static (Mock<IHubContext<NotificationsHub>> hub, Mock<IClientProxy> clientProxy) CreateHubContextMock()
    {
        var hubContextMock = new Mock<IHubContext<NotificationsHub>>();
        var clientProxyMock = new Mock<IClientProxy>();
        clientProxyMock
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
        hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        return (hubContextMock, clientProxyMock);
    }

    private static Booking CreateBooking(Guid bookingId, Guid itemId, Guid borrowerId, Guid ownerId, Status status)
    {
        return new Booking(
            bookingId,
            itemId,
            borrowerId,
            ownerId,
            status,
            DateTime.UtcNow.AddDays(-2),
            DateTime.UtcNow.AddDays(2),
            default,
            25m,
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(-2),
            default);
    }

    [Fact]
    public async Task Handle_ReturnsUnauthorized_WhenUserMissing()
    {
        var context = CreateContext();
        var (hubMock, _) = CreateHubContextMock();
        var handler = new CompleteBookingHandler(context, CreateHttpContextAccessor(), hubMock.Object);

        var result = await handler.Handle(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status401Unauthorized, (result as IStatusCodeHttpResult)?.StatusCode);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenBookingMissing()
    {
        var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var (hubMock, _) = CreateHubContextMock();
        var handler = new CompleteBookingHandler(context, CreateHttpContextAccessor(ownerId), hubMock.Object);

        var result = await handler.Handle(Guid.NewGuid());

        Assert.Equal(StatusCodes.Status404NotFound, (result as IStatusCodeHttpResult)?.StatusCode);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenItemMissing()
    {
        var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var booking = CreateBooking(bookingId, Guid.NewGuid(), Guid.NewGuid(), ownerId, Status.Approved);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var (hubMock, _) = CreateHubContextMock();
        var handler = new CompleteBookingHandler(context, CreateHttpContextAccessor(ownerId), hubMock.Object);

        var result = await handler.Handle(bookingId);

        Assert.Equal(StatusCodes.Status404NotFound, (result as IStatusCodeHttpResult)?.StatusCode);
    }

    [Fact]
    public async Task Handle_ReturnsForbid_WhenUserIsNotOwner()
    {
        var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var otherUser = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var booking = CreateBooking(bookingId, itemId, Guid.NewGuid(), ownerId, Status.Approved);
        context.Bookings.Add(booking);
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, "img", true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var (hubMock, _) = CreateHubContextMock();
        var handler = new CompleteBookingHandler(context, CreateHttpContextAccessor(otherUser), hubMock.Object);

        var result = await handler.Handle(bookingId);

        Assert.Contains("Forbid", result.GetType().Name);
    }

    [Fact]
    public async Task Handle_ReturnsBadRequest_For_Invalid_Status()
    {
        var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Books, Infrastructure.Features.Items.Condition.New, 10m, "img", false, DateTime.UtcNow));
        context.Bookings.Add(CreateBooking(bookingId, itemId, Guid.NewGuid(), ownerId, Status.Pending));
        await context.SaveChangesAsync();

        var (hubMock, _) = CreateHubContextMock();
        var handler = new CompleteBookingHandler(context, CreateHttpContextAccessor(ownerId), hubMock.Object);

        var result = await handler.Handle(bookingId);

        Assert.Equal(StatusCodes.Status400BadRequest, (result as IStatusCodeHttpResult)?.StatusCode);
    }

    [Fact]
    public async Task Handle_Completes_Booking_And_Marks_Item_Available()
    {
        var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        var booking = CreateBooking(bookingId, itemId, borrowerId, ownerId, Status.Approved);
        context.Bookings.Add(booking);
        context.Items.Add(new Infrastructure.Features.Items.Item(itemId, ownerId, "Item", "Desc", Infrastructure.Features.Items.Category.Electronics, Infrastructure.Features.Items.Condition.WellPreserved, 15m, "img", false, DateTime.UtcNow.AddDays(-1)));
        await context.SaveChangesAsync();

        var (hubMock, clientProxyMock) = CreateHubContextMock();
        var handler = new CompleteBookingHandler(context, CreateHttpContextAccessor(ownerId), hubMock.Object);

        var result = await handler.Handle(bookingId);

        Assert.Equal(StatusCodes.Status200OK, (result as IStatusCodeHttpResult)?.StatusCode);

        var updatedBooking = await context.Bookings.SingleAsync(b => b.Id == bookingId);
        var updatedItem = await context.Items.SingleAsync(i => i.Id == itemId);

        Assert.Equal(Status.Completed, updatedBooking.Status);
        Assert.NotEqual(default, updatedBooking.ActualReturnDate);
        Assert.NotEqual(default, updatedBooking.CompletedAt);
        Assert.True(updatedItem.IsAvailable);

        clientProxyMock.Verify(p => p.SendCoreAsync("BookingUpdated", It.Is<object?[]>(o => ContainsCompletedBooking(bookingId, o)), It.IsAny<CancellationToken>()), Times.Once);
        clientProxyMock.Verify(p => p.SendCoreAsync("ItemUpdated", It.Is<object?[]>(o => ContainsUpdatedItem(itemId, o)), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static bool ContainsCompletedBooking(Guid bookingId, object?[] args)
    {
        if (args.Length != 1)
            return false;

        if (args[0] is not Booking booking)
            return false;

        return booking.Id == bookingId && booking.Status == Status.Completed;
    }

    private static bool ContainsUpdatedItem(Guid itemId, object?[] args)
    {
        if (args.Length != 1)
            return false;

        if (args[0] is not Infrastructure.Features.Items.Item item)
            return false;

        return item.Id == itemId && item.IsAvailable;
    }
}
