using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

namespace UniShare.Infrastructure.Features.Items.CreateItem;

public class CreateItemHandler(
    UniShareContext context,
    IValidator<CreateItemRequest> validator,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<IResult> Handle(CreateItemRequest request)
    {
        Log.Info($"CreateItem request received: Title={request.Title}, Categ={request.Categ}, Cond={request.Cond}");
        
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            Log.Error($"Validation failed for CreateItemRequest: {string.Join(", ", validationResult.Errors.Select(e => $"{e.PropertyName}={e.ErrorMessage}"))}");
            return Results.ValidationProblem(errors);
        }

        var (userId, errorResult) = await GetAuthenticatedUserIdAsync();
        if (errorResult is not null) return errorResult;

        var item = new Item(
            Guid.NewGuid(),
            userId!.Value,
            request.Title,
            request.Description,
            request.Categ,
            request.Cond,
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

    private async Task<(Guid? UserId, IResult? ErrorResult)> GetAuthenticatedUserIdAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            Log.Error("HttpContext is null");
            return (null, Results.Unauthorized());
        }

        if (!httpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
        {
            Log.Error($"UserId not found in context or is not a Guid. Value: {userIdObj}");
            return (null, Results.Unauthorized());
        }

        var ownerExists = await context.Users.AnyAsync(u => u.Id == userId);
        if (!ownerExists)
        {
            Log.Error($"Owner with id: {userId} does not exist");
            return (null, Results.BadRequest(new { errors = new { OwnerId = new[] { "Owner does not exist." } } }));
        }

        Log.Info($"Successfully authenticated user with id: {userId}");
        return (userId, null);
    }
}
