using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Bookings;

public class BookingBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UniShareContext>();
                await MarkExpiredBookingsAsExpired(dbContext);
                await MarkActiveBookingsAsActive(dbContext);
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private static async Task MarkExpiredBookingsAsExpired(UniShareContext dbContext)
    {
        var expiredBookings = await dbContext.Bookings
            .Where(b => b.Status == Status.Active && b.EndDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var booking in expiredBookings)
        {
            var updatedBooking = booking with { Status = Status.Expired };
            dbContext.Entry(booking).CurrentValues.SetValues(updatedBooking);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task MarkActiveBookingsAsActive(UniShareContext dbContext)
    {
        var activeBookings = await dbContext.Bookings
            .Where(b => b.Status == Status.Approved && b.StartDate <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var booking in activeBookings)
        {
            var updatedBooking = booking with { Status = Status.Active };
            dbContext.Entry(booking).CurrentValues.SetValues(updatedBooking);
        }

        await dbContext.SaveChangesAsync();
    }
}