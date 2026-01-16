using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class BookingBackgroundServiceTests
{
    private static UniShareContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    private static async Task InvokePrivateAsync(string methodName, UniShareContext context)
    {
        var method = typeof(BookingBackgroundService).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var task = method!.Invoke(null, new object?[] { context }) as Task;
        Assert.NotNull(task);

        await task!;
    }

    [Fact]
    public async Task MarkExpiredBookingsAsExpired_Updates_Expired_Status()
    {
        var context = CreateContext();
        var expired = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Active,
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(-1),
            default,
            10m,
            DateTime.UtcNow.AddDays(-4),
            default,
            default);
        context.Bookings.Add(expired);
        await context.SaveChangesAsync();

        await InvokePrivateAsync("MarkExpiredBookingsAsExpired", context);

        var updated = await context.Bookings.FirstAsync();
        Assert.Equal(Status.Expired, updated.Status);
    }

    [Fact]
    public async Task MarkActiveBookingsAsActive_Updates_Approved_Bookings()
    {
        var context = CreateContext();
        var shouldActivate = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Approved,
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow.AddDays(1),
            default,
            20m,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            default);
        var futureBooking = new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Approved,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(3),
            default,
            15m,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            default);

        context.Bookings.AddRange(shouldActivate, futureBooking);
        await context.SaveChangesAsync();

        await InvokePrivateAsync("MarkActiveBookingsAsActive", context);

        var updated = await context.Bookings.SingleAsync(b => b.Id == shouldActivate.Id);
        var untouched = await context.Bookings.SingleAsync(b => b.Id == futureBooking.Id);

        Assert.Equal(Status.Active, updated.Status);
        Assert.Equal(Status.Approved, untouched.Status);
    }
}
