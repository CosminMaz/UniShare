namespace UniShare.Infrastructure.Features.Reviews;

public enum ReviewType
{
    Bad = 1,
    Ok = 2,
    Good = 3,
    VeryGood = 4
}

public record Review(Guid Id, Guid BookingId, Guid ReviewerId, Guid ItemId, int Rating, string Comment, ReviewType RevType, DateTime CreatedAt);