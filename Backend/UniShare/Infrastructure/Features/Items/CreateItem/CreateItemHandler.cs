using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

namespace UniShare.Infrastructure.Features.Items.CreateItem;

public class CreateItemHandler(
    UniShareContext context,
    IValidator<CreateItemRequest> validator)
{
    public async Task<IResult> Handle(CreateItemRequest request)
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
            
            Log.Error("Validation failed for CreateItemRequest");
            return Results.ValidationProblem(errors);
        }

        var ownerExists = await context.Users.AnyAsync(u => u.Id == request.OwnerId);
        if (!ownerExists)
        {
            Log.Error($"Owner with id: {request.OwnerId} does not exist");
            return Results.BadRequest(new
            {
                errors = new
                {
                    OwnerId = new[] { "Owner does not exist." }
                }
            });
        }

        var item = new Item(
            Guid.NewGuid(),
            request.OwnerId,
            request.Title,
            request.Description,
            request.Category,
            request.Condition,
            request.DailyRate,
            request.ImageUrl,
            true,
            DateTime.UtcNow
        );

        context.Items.Add(item);
        await context.SaveChangesAsync();
        
        Log.Info($"Item with id: {item.Id} was created");
        return Results.Created($"/items/{item.Id}", item);
    }
}
