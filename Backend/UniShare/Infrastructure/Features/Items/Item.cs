namespace UniShare.Infrastructure.Features.Items;

public record Item(Guid Id, Guid OwnerId, string Title, string Description, string Category, string Condition, decimal? DailyRate, string? ImageUrl, bool IsAvailable, DateTime CreatedAt);