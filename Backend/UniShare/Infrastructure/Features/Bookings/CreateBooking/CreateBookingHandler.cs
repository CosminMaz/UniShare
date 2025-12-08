using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;
using UniShare.Infrastructure.Features.Items;

namespace UniShare.Infrastructure.Features.Bookings.CreateBooking;

public class CreateBookingHandler(
    UniShareContext context,
    IValidator<CreateBookingRequest> validator,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<IResult> Handle(CreateBookingRequest request)
    {
        Log.Info($"CreateBooking request received: ItemId={request.ItemId}, StartDate={request.StartDate}, EndDate={request.EndDate}");

        var validationProblem = await ValidateRequest(request);
        if (validationProblem is not null)
            return validationProblem;

        var borrowerId = GetUserId();
        if (!borrowerId.HasValue)
            return Results.Unauthorized();

        var item = await GetItem(request.ItemId);
        if (item is null)
            return ItemDoesNotExistResult();

        if (!item.IsAvailable)
            return ItemUnavailableResult();

        var ownerId = item.OwnerId;
        if (!await UserExists(ownerId))
            return OwnerDoesNotExistResult();

        var booking = await CreateBooking(request, borrowerId.Value, ownerId, item);

        return Results.Created($"/bookings/{booking.Id}", booking);
    }
    
    private async Task<IResult?> ValidateRequest(CreateBookingRequest request)
    {
        var validation = await validator.ValidateAsync(request);
        if (validation.IsValid) 
            return null;

        var errors = validation.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        Log.Error("Validation failed");
        return Results.ValidationProblem(errors);
    }

    private Guid? GetUserId()
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx == null) return null;

        ctx.Items.TryGetValue("UserId", out var userIdObj);
        return userIdObj is Guid id ? id : null;
    }

    private async Task<bool> UserExists(Guid id)
        => await context.Users.AnyAsync(u => u.Id == id);

    private async Task<Item?> GetItem(Guid id)
        => await context.Items.FirstOrDefaultAsync(i => i.Id == id);

    private static IResult ItemDoesNotExistResult()
        => Results.BadRequest(new { errors = new { ItemId = new[] { "Item does not exist." } } });

    private static IResult ItemUnavailableResult()
        => Results.BadRequest(new { errors = new { ItemId = new[] { "Item is not available." } } });

    private static IResult OwnerDoesNotExistResult()
        => Results.BadRequest(new { errors = new { OwnerId = new[] { "Owner does not exist." } } });

    private async Task<Booking> CreateBooking(
        CreateBookingRequest request,
        Guid borrowerId,
        Guid ownerId,
        Item item)
    {
        var totalPrice = CalculatePrice(item.DailyRate, request.StartDate, request.EndDate);

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
            DateTime.UtcNow,
            default,
            default
        );

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        Log.Info($"Booking created: {booking.Id}");
        return booking;
    }

    private static decimal CalculatePrice(decimal? rate, DateTime start, DateTime end)
    {
        if (!rate.HasValue) return 0m;

        var days = Math.Max(1, (end.Date - start.Date).TotalDays);
        return rate.Value * (decimal)days;
    }
}
