using FluentValidation;
using UniShare.Infrastructure.Features.Bookings.ApproveBooking;

namespace UniShare.Infrastructure.Validators;

public class ApproveBookingValidator : AbstractValidator<ApproveBookingRequest>
{
    public ApproveBookingValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("BookingId is required.");
    }
}

