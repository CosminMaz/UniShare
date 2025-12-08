namespace UniShare.Infrastructure.Features.Bookings.CreateBooking;

public record CreateBookingRequest(Guid ItemId, DateTime StartDate, DateTime EndDate);