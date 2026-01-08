using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;
using UniShare.Infrastructure.Features.Bookings;

namespace UniShare.Infrastructure.Features.Reviews.CreateReview;

public class CreateReviewHandler(
    UniShareContext context,
    IValidator<CreateReviewRequest> validator,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<IResult> Handle(CreateReviewRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            Log.Error("Validation failed for CreateReviewRequest");
            return Results.ValidationProblem(errors);
        }

        var currentUserId = GetUserId();
        if (!currentUserId.HasValue)
            return Results.Unauthorized();

        var booking = await context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == request.BookingId);
        if (booking is null)
            return Results.BadRequest(new { error = "Booking not found." });

        if (booking.BorrowerId != currentUserId.Value)
            return Results.Forbid();

        if (booking.Status != Status.Completed)
            return Results.BadRequest(new { error = "Reviews can only be left for completed bookings." });

        var existingReview = await context.Reviews.AnyAsync(r => r.BookingId == request.BookingId);
        if (existingReview)
            return Results.Conflict(new { error = "A review for this booking already exists." });
        
        var review = new Review(
            Guid.NewGuid(),
            request.BookingId,
            currentUserId.Value,
            booking.ItemId,
            request.Rating,
            request.Comment,
            request.RevType!.Value,
            DateTime.UtcNow
        );

        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        Log.Info($"Review with id: {review.Id} was created");
        return Results.Created($"/reviews/{review.Id}", review);
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
}
