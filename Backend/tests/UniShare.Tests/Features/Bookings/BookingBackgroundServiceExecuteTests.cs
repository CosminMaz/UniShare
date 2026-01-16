using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class BookingBackgroundServiceExecuteTests
{
    private class TestBookingBackgroundService(IServiceProvider serviceProvider) : BookingBackgroundService(serviceProvider)
    {
        public Task RunAsync(CancellationToken token) => ExecuteAsync(token);
    }

    [Fact]
    public async Task ExecuteAsync_Processes_Bookings_And_Stops_On_Cancellation()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contextInstance = new UniShareContext(options);

        var services = new ServiceCollection();
        services.AddSingleton(contextInstance);
        await using var provider = services.BuildServiceProvider();

        await SeedBookingsAsync(contextInstance);

        var service = new TestBookingBackgroundService(provider);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await using (var initialScope = provider.CreateAsyncScope())
        {
            var initialContext = initialScope.ServiceProvider.GetRequiredService<UniShareContext>();
            Assert.Equal(2, await initialContext.Bookings.CountAsync());
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        await using var verificationScope = provider.CreateAsyncScope();
        var context = verificationScope.ServiceProvider.GetRequiredService<UniShareContext>();
        var bookings = await context.Bookings.ToListAsync();

        Assert.Contains(bookings, b => b.Status == Status.Expired && b.EndDate < DateTime.UtcNow);
        Assert.Contains(bookings, b => b.Status == Status.Active && b.StartDate <= DateTime.UtcNow);
    }

    private static async Task SeedBookingsAsync(UniShareContext context)
    {
        context.Bookings.Add(new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Active,
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-1),
            default,
            10m,
            DateTime.UtcNow.AddDays(-6),
            default,
            default));

        context.Bookings.Add(new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Status.Approved,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddDays(2),
            default,
            20m,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            default));

        await context.SaveChangesAsync();
    }
}
