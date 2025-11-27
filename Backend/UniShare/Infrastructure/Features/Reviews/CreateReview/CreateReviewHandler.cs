using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Reviews.CreateReview;

public class CreateReviewHandler(
    UniShareContext context,
    IValidator<CreateReviewRequest> validator)
{
    public async Task<IResult> Handle(CreateReviewRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            Log.Error("Validation failed for CreateReviewRequest");
            return Results.ValidationProblem(errors);
        }

        var reviewerExists = await context.Users.AnyAsync(u => u.Id == request.ReviewerId);
        if (!reviewerExists)
        {
            Log.Error($"Reviewer with id: {request.ReviewerId} does not exist");
            return Results.BadRequest(new
            {
                errors = new
                {
                    ReviewerId = new[] { "Reviewer does not exist." }
                }
            });
        }

        var itemExists = await context.Items.AnyAsync(i => i.Id == request.ItemId);
        if (!itemExists)
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

        /*
        var bookingExists = await context.Bookings.AnyAsync(b => b.Id == request.BookingId);
        if (!bookingExists)
        {
            Log.Error($"Booking with id: {request.BookingId} does not exist");
            return Results.BadRequest(new
            {
                errors = new
                {
                    BookingId = new[] { "Booking does not exist." }
                }
            });
        }
        */
        // TODO: enable booking existence check after Bookings API is implemented.

        var review = new Review(
            Guid.NewGuid(),
            request.BookingId,
            request.ReviewerId,
            request.ItemId,
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
}
