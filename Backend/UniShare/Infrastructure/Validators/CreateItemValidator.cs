using FluentValidation;
using UniShare.Infrastructure.Features.Items.CreateItem;

namespace UniShare.Infrastructure.Validators;

public class CreateItemValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).WithMessage("Title must be at least 3 characters long.");
        RuleFor(x => x.Description).NotEmpty().MinimumLength(5).WithMessage("Description must be at least 5 characters long.");
        RuleFor(x => x.Categ).IsInEnum().WithMessage("Category must be a valid value.");
        RuleFor(x => x.Cond).IsInEnum().WithMessage("Condition must be a valid value.");
        RuleFor(x => x.DailyRate).GreaterThan(0).When(x => x.DailyRate.HasValue).WithMessage("DailyRate must be a positive value.");
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("ImageUrl is required.");
    }
}
