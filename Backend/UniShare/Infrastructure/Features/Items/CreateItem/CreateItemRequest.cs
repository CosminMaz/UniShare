namespace UniShare.Infrastructure.Features.Items.CreateItem;

public record CreateItemRequest(string Title, string Description, Category Categ, Condition Cond, decimal? DailyRate, string? ImageUrl);