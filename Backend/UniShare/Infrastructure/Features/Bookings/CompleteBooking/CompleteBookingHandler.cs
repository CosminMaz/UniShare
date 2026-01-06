using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;
using UniShare.Infrastructure.Features.Items;
using UniShare.RealTime;

namespace UniShare.Infrastructure.Features.Bookings.CompleteBooking;

public class CompleteBookingHandler(
    UniShareContext context,
    IHttpContextAccessor httpContextAccessor,
    IHubContext<NotificationsHub> hubContext)
{
    public async Task<IResult> Handle(Guid bookingId)
    {
        var currentUserId = GetUserId();
        if (!currentUserId.HasValue)
            return Results.Unauthorized();

        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null)
            return Results.NotFound(new { error = "Booking not found." });

        var item = await context.Items.FirstOrDefaultAsync(i => i.Id == booking.ItemId);
        if (item is null)
            // This case should ideally not happen if database integrity is maintained
            return Results.NotFound(new { error = "Associated item not found." });

        if (item.OwnerId != currentUserId.Value)
            return Results.Forbid();

        if (booking.Status is not Status.Approved and not Status.Active)
        {
            return Results.BadRequest(new { error = $"Booking cannot be completed because its status is '{booking.Status}'." });
        }

        var updatedBooking = booking with
        {
            Status = Status.Completed,
            ActualReturnDate = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        context.Entry(booking).CurrentValues.SetValues(updatedBooking);

        var updatedItem = await MarkItemAvailable(booking.ItemId);

        await context.SaveChangesAsync();

        await hubContext.Clients.All.SendAsync("BookingUpdated", updatedBooking);
        if (updatedItem is not null)
        {
            await hubContext.Clients.All.SendAsync("ItemUpdated", updatedItem);
        }

        Log.Info($"Booking {bookingId} has been marked as completed by owner {currentUserId.Value}");

        return Results.Ok(updatedBooking);
    }

    private Guid? GetUserId()
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx?.Items is null || !ctx.Items.TryGetValue("UserId", out var userIdObj))
        {
            return null;
        }
        return userIdObj as Guid?;
    }

    private async Task<Item?> MarkItemAvailable(Guid itemId)
    {
        var item = await context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null)
        {
            Log.Warning($"Item {itemId} not found while completing booking.");
            return null;
        }

        if (item.IsAvailable)
            return item;

        var updatedItem = item with { IsAvailable = true };
        context.Entry(item).CurrentValues.SetValues(updatedItem);
        return updatedItem;
    }
}
