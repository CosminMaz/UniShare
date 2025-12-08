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

        var reviewerCheck = await ValidateEntityExists(context.Users, request.ReviewerId, "Reviewer");
        if (reviewerCheck is not null)
        {
            return reviewerCheck;
        }

        var itemCheck = await ValidateEntityExists(context.Items, request.ItemId, "Item");
        if (itemCheck is not null)
        {
            return itemCheck;
        }
     
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

    private static async Task<IResult?> ValidateEntityExists<T>(IQueryable<T> queryable, Guid id, string entityName) where T : class
    {
        if (await queryable.AnyAsync(e => EF.Property<Guid>(e, "Id") == id)) return null;
        Log.Error($"{entityName} with id: {id} does not exist");
        return Results.BadRequest(new
        {
            errors = new Dictionary<string, string[]>
            {
                { $"{entityName}Id", [$"{entityName} does not exist."] }
            }
        });

    }
}
