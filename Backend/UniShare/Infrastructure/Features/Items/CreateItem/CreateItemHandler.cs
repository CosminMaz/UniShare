using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniShare.Infrastructure.Persistence;

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

            return Results.ValidationProblem(errors);
        }

        var ownerExists = await context.Users.AnyAsync(u => u.Id == request.OwnerId);
        if (!ownerExists)
        {
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
        
        return Results.Created($"/items/{item.Id}", item);
    }
}
