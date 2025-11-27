using FluentValidation;
using UniShare.Infrastructure.Features.Reviews.CreateReview;

namespace UniShare.Infrastructure.Validators;

public class CreateReviewValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewValidator()
    {
        RuleFor(r => r.BookingId)
            .NotEmpty().WithMessage("BookingId is required.");

        RuleFor(r => r.ReviewerId)
            .NotEmpty().WithMessage("ReviewerId is required.");

        RuleFor(r => r.ItemId)
            .NotEmpty().WithMessage("ItemId is required.");

        RuleFor(r => r.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

        RuleFor(r => r.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(500).WithMessage("Comment must be at most 500 characters.");

        RuleFor(r => r.RevType)
            .NotNull().WithMessage("Review type is required.");
    }
}