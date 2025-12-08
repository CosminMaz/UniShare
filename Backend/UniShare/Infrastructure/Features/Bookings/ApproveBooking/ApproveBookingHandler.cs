using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Bookings.ApproveBooking;

public class ApproveBookingHandler(
    UniShareContext context,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<IResult> Handle(ApproveBookingRequest request)
    {
        Log.Info($"ApproveBooking request received: BookingId={request.BookingId}, Approve={request.Approve}");

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            Log.Error("HttpContext is null");
            return Results.Unauthorized();
        }

        httpContext.Items.TryGetValue("UserId", out var userIdObj);
        if (userIdObj is not Guid ownerId)
        {
            Log.Error($"UserId not found in context or not a Guid. Value: {userIdObj}");
            return Results.Unauthorized();
        }

        // Retrieve booking as a tracked entity
        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
        {
            Log.Error($"Booking with id: {request.BookingId} does not exist");
            return Results.NotFound(new { message = "Booking not found." });
        }

        // Ensure the logged-in user is the owner of the item
        if (booking.OwnerId != ownerId)
        {
            Log.Error($"User {ownerId} is not the owner of booking {request.BookingId}");
            return Results.Forbid();
        }

        // Booking must be in Pending state before a decision can be made
        if (booking.Status != Status.Pending)
        {
            Log.Error($"Booking {request.BookingId} is not in Pending status. Current status: {booking.Status}");
            return Results.BadRequest(new { message = "Only pending bookings can be approved or rejected." });
        }

        var now = DateTime.UtcNow;
        var newStatus = request.Approve ? Status.Approved : Status.Rejected;

        // Create a new immutable instance with updated status and timestamps
        var updatedBooking = booking with { Status = newStatus, ApprovedAt = request.Approve ? now : booking.ApprovedAt };

        // Update tracked entity using EF Core value copying
        context.Entry(booking).CurrentValues.SetValues(updatedBooking);

        // If approved, mark the associated item as unavailable
        if (request.Approve)
        {
            var item = await context.Items.FirstOrDefaultAsync(i => i.Id == booking.ItemId);
            if (item != null)
            {
                var updatedItem = new Item(
                    item.Id,
                    item.OwnerId,
                    item.Title,
                    item.Description,
                    item.Categ,
                    item.Cond,
                    item.DailyRate,
                    item.ImageUrl,
                    false, // IsAvailable = false
                    item.CreatedAt
                );

                context.Entry(item).CurrentValues.SetValues(updatedItem);
            }
        }

        await context.SaveChangesAsync();

        // Fetch updated booking without tracking for response clarity
        var resultBooking = await context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        Log.Info($"Booking {request.BookingId} was {(request.Approve ? "approved" : "rejected")} by owner {ownerId}");
        return Results.Ok(resultBooking);
    }
}
