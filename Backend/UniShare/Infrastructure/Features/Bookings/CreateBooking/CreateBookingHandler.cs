using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

namespace UniShare.Infrastructure.Features.Bookings.CreateBooking;

public class CreateBookingHandler(
    UniShareContext context,
    IValidator<CreateBookingRequest> validator,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<IResult> Handle(CreateBookingRequest request)
    {
        Log.Info($"CreateBooking request received: ItemId={request.ItemId}, StartDate={request.StartDate}, EndDate={request.EndDate}");
        
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            Log.Error($"Validation failed for CreateBookingRequest: {string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}={e.ErrorMessage}"))}");
            return Results.ValidationProblem(errors);
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            Log.Error("HttpContext is null");
            return Results.Unauthorized();
        }

        httpContext.Items.TryGetValue("UserId", out var userIdObj);
        Log.Info($"UserId from context: {userIdObj}");

        if (userIdObj is not Guid borrowerId)
        {
            Log.Error($"UserId not found in context or not a Guid. Value: {userIdObj}");
            return Results.Unauthorized();
        }

        var borrowerExists = await context.Users.AnyAsync(u => u.Id == borrowerId);
        if (!borrowerExists)
        {
            Log.Error($"Borrower with id: {borrowerId} does not exist");
            return Results.BadRequest(new
            {
                errors = new
                {
                    BorrowerId = new[] { "Borrower does not exist." }
                }
            });
        }

        var item = await context.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId);
        if (item == null)
        {
            Log.Error($"Item with id: {request.ItemId} does not exist");
            return Results.BadRequest(new
            {
                errors = new
                {
                    ItemId = new[] { "Item does not exist." }
                }
            });
        }

        if (!item.IsAvailable)
        {
            Log.Error($"Item with id: {request.ItemId} is not available");
            return Results.BadRequest(new
            {
                errors = new
                {
                    ItemId = new[] { "Item is not available." }
                }
            });
        }

        var ownerId = item.OwnerId;
        var ownerExists = await context.Users.AnyAsync(u => u.Id == ownerId);
        if (!ownerExists)
        {
            Log.Error($"Owner with id: {ownerId} does not exist");
            return Results.BadRequest(new
            {
                errors = new
                {
                    OwnerId = new[] { "Owner does not exist." }
                }
            });
        }

        decimal totalPrice = 0m;
        if (item.DailyRate.HasValue)
        {
            var days = (request.EndDate.Date - request.StartDate.Date).TotalDays;
            if (days < 1) days = 1;
            totalPrice = item.DailyRate.Value * (decimal)days;
        }

        var now = DateTime.UtcNow;

        var booking = new Booking(
            Guid.NewGuid(),
            request.ItemId,
            borrowerId,
            ownerId,
            Status.Pending,
            request.StartDate,
            request.EndDate,
            default,
            totalPrice,
            now,
            default,
            default
        );

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        Log.Info($"Booking with id: {booking.Id} was created");
        return Results.Created($"/bookings/{booking.Id}", booking);
    }
}
