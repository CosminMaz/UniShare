namespace UniShare.Infrastructure.Features.Items;

public enum Category
{
    Books = 1,
    Electronics = 2,
    Clothing = 3,
    Furniture = 4,
    Sports = 5,
    Other = 6
}

public enum Condition
{
    New = 1,
    LikeNew = 2,
    WellPreserved = 3,
    Acceptable = 4,
    Poor = 5
}

public record Item(Guid Id, Guid OwnerId, string Title, string Description, Category Categ, Condition Cond, decimal? DailyRate, string? ImageUrl, bool IsAvailable, DateTime CreatedAt);