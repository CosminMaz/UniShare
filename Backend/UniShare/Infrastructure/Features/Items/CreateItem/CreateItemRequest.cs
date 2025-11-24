namespace UniShare.Infrastructure.Features.Items;

public record CreateItemRequest(Guid OwnerId,string Title, string Description, string Category, string Condition, decimal? DailyRate, string? ImageUrl);