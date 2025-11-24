using FluentValidation;
using UniShare.Infrastructure.Features.Items;

namespace UniShare.Infrastructure.Validators;

public class CreateItemValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required.");
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).WithMessage("Title must be at least 3 characters long.");
        RuleFor(x => x.Description).NotEmpty().MinimumLength(10).WithMessage("Description must be at least 10 characters long.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.Condition).NotEmpty().WithMessage("Condition is required.");
        RuleFor(x => x.DailyRate).GreaterThanOrEqualTo(0).When(x => x.DailyRate.HasValue).WithMessage("DailyRate must be a positive value.");
    }
}
