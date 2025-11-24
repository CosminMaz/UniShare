using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Items.CreateItem;

public class CreateItemHandler
{
    private readonly UniShareContext _context;
    private readonly ILogger<CreateItemHandler> _logger;
    private readonly IValidator<CreateItemRequest> _validator;

    public CreateItemHandler(
        UniShareContext context,
        ILogger<CreateItemHandler> logger,
        IValidator<CreateItemRequest> validator)
    {
        _context = context;
        _logger = logger;
        _validator = validator;
    }

    public async Task<IResult> Handle(CreateItemRequest request)
    {
        _logger.LogInformation("Creating item for owner {OwnerId} with title: {Title}", request.OwnerId, request.Title);

        var validationResult = await _validator.ValidateAsync(request);
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

        var ownerExists = await _context.Users.AnyAsync(u => u.Id == request.OwnerId);
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

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Item created successfully with ID: {ItemId}", item.Id);

        return Results.Created($"/items/{item.Id}", item);
    }
}
