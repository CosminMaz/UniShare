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

        var ownerId = GetUserId();
        if (ownerId is null)
            return Results.Unauthorized();

        var booking = await GetBooking(request.BookingId);
        if (booking is null)
            return Results.NotFound(new { message = "Booking not found." });

        var authorizationResult = ValidateOwner(booking, ownerId.Value);
        if (authorizationResult is not null)
            return authorizationResult;

        var statusValidation = ValidateBookingStatus(booking);
        if (statusValidation is not null)
            return statusValidation;

        await ApplyDecisionAsync(booking, request.Approve);

        var updatedBooking = await GetBookingReadonly(request.BookingId);
        return Results.Ok(updatedBooking);
    }

    // ---------------------- HELPERS ----------------------

    private Guid? GetUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            Log.Error("HttpContext is null");
            return null;
        }

        httpContext.Items.TryGetValue("UserId", out var userIdObj);
        return userIdObj is Guid g ? g : null;
    }

    private Task<Booking?> GetBooking(Guid id) =>
        context.Bookings.FirstOrDefaultAsync(b => b.Id == id);

    private Task<Booking?> GetBookingReadonly(Guid id) =>
        context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    private static IResult? ValidateOwner(Booking booking, Guid ownerId)
    {
        if (booking.OwnerId == ownerId) return null;
        Log.Error($"User {ownerId} is not the owner of booking {booking.Id}");
        return Results.Forbid();

    }

    private static IResult? ValidateBookingStatus(Booking booking)
    {
        if (booking.Status == Status.Pending) return null;
        Log.Error($"Booking {booking.Id} is not pending. Current status: {booking.Status}");
        return Results.BadRequest(new { message = "Only pending bookings can be approved or rejected." });

    }

    private async Task ApplyDecisionAsync(Booking booking, bool approve)
    {
        var now = DateTime.UtcNow;
        var newStatus = approve ? Status.Approved : Status.Rejected;

        // Update booking
        var updatedBooking = booking with
        {
            Status = newStatus,
            ApprovedAt = approve ? now : booking.ApprovedAt
        };

        context.Entry(booking).CurrentValues.SetValues(updatedBooking);

        if (approve)
            await MarkItemUnavailable(booking.ItemId);

        await context.SaveChangesAsync();

        Log.Info($"Booking {booking.Id} was {(approve ? "approved" : "rejected")}");
    }

    private async Task MarkItemUnavailable(Guid itemId)
    {
        var item = await context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item == null) return;

        var updatedItem = item with { IsAvailable = false };

        context.Entry(item).CurrentValues.SetValues(updatedItem);
    }
}
