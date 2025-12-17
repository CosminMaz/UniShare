namespace UniShare.Infrastructure.Features.Bookings;

/*
 * enum: Pending, Approved, Active, Completed, Cancelled, Rejected, Expired
 */

public enum Status
{
    Pending = 1,
    Approved = 2,
    Active = 3,
    Completed = 4,
    Canceled = 5,
    Rejected = 6,
    Expired = 7
}

public record Booking(Guid Id, Guid ItemId, Guid BorrowerId, Guid OwnerId, Status Status, DateTime StartDate, DateTime EndDate, DateTime ActualReturnDate, Decimal TotalPrice, DateTime RequestedAt, DateTime ApprovedAt, DateTime CompletedAt);