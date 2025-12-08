using FluentValidation;
using UniShare.Infrastructure.Features.Bookings.CreateBooking;

namespace UniShare.Infrastructure.Validators;

public class CreateBookingValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty()
            .WithMessage("ItemId is required.");

        RuleFor(x => x.StartDate)
            .Must(d => d != default)
            .WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .Must(d => d != default)
            .WithMessage("EndDate is required.");

        RuleFor(x => x)
            .Must(x => x.EndDate > x.StartDate)
            .WithMessage("EndDate must be after StartDate.")
            .When(x => x.StartDate != default && x.EndDate != default);

        RuleFor(x => x.StartDate)
            .Must(d => d.Date >= DateTime.UtcNow.Date)
            .WithMessage("StartDate cannot be in the past.")
            .When(x => x.StartDate != default);
    }
}


