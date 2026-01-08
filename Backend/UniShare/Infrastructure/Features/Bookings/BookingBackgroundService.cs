using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

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
        try
        {
            var expiredBookings = await dbContext.Bookings
                .Where(b => b.Status == Status.Active && b.EndDate < DateTime.UtcNow)
                .ToListAsync();

            if (expiredBookings.Count == 0)
                return;

            foreach (var booking in expiredBookings)
            {
                var updatedBooking = booking with { Status = Status.Expired };
                dbContext.Entry(booking).CurrentValues.SetValues(updatedBooking);
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            // The database constraint may not include 'Expired' status
            Log.Error($"Error marking expired bookings: {ex.Message}");
        }
    }

    private static async Task MarkActiveBookingsAsActive(UniShareContext dbContext)
    {
        try
        {
            var activeBookings = await dbContext.Bookings
                .Where(b => b.Status == Status.Approved && b.StartDate <= DateTime.UtcNow)
                .ToListAsync();

            if (activeBookings.Count == 0)
                return;

            foreach (var booking in activeBookings)
            {
                var updatedBooking = booking with { Status = Status.Active };
                dbContext.Entry(booking).CurrentValues.SetValues(updatedBooking);
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            Log.Error($"Error marking active bookings: {ex.Message}");
        }
    }
}