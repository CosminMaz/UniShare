namespace UniShare.Infrastructure.Features.Bookings.ApproveBooking;

public record ApproveBookingRequest(Guid BookingId, bool Approve);

