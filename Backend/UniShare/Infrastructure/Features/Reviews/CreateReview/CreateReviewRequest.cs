namespace UniShare.Infrastructure.Features.Reviews.CreateReview;

public record CreateReviewRequest(Guid BookingId, Guid ReviewerId, Guid ItemId, int Rating, string Comment, ReviewType? RevType);