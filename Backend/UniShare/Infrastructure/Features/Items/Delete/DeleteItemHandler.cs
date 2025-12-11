using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Items.Delete;

/// <summary>
/// Handles deleting an item. Only the item owner (based on HttpContext UserId) can delete.
/// </summary>
public class DeleteItemHandler(
    UniShareContext context,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<IResult> Handle(Guid itemId)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            Log.Error("DeleteItem failed: HttpContext is null");
            return Results.Unauthorized();
        }

        httpContext.Items.TryGetValue("UserId", out var userIdObj);
        if (userIdObj is not Guid userId)
        {
            Log.Error("DeleteItem failed: UserId not found in context or invalid");
            return Results.Unauthorized();
        }

        var item = await context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null)
        {
            Log.Error($"DeleteItem failed: item {itemId} not found");
            return Results.NotFound();
        }

        if (item.OwnerId != userId)
        {
            Log.Error($"DeleteItem failed: user {userId} is not owner of item {itemId}");
            return Results.Forbid();
        }

        // Prevent deletion when bookings reference this item to avoid FK violations
        var hasBookings = await context.Bookings.AnyAsync(b => b.ItemId == itemId);
        if (hasBookings)
        {
            Log.Error($"DeleteItem failed: item {itemId} has related bookings");
            return Results.Conflict(new { message = "Cannot delete item because it has existing bookings." });
        }

        context.Items.Remove(item);
        await context.SaveChangesAsync();

        Log.Info($"Item {itemId} deleted by owner {userId}");
        return Results.NoContent();
    }
}

