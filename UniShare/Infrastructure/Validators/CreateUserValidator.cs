using FluentValidation;
using UniShare.Infrastructure.Features.Users;
namespace UniShare.Infrastructure.Validators;

public class CreateUserValidator:AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x=> x.Fullname).NotNull().NotEmpty().MinimumLength(3).WithMessage("FullName must be at least 3 characters long.");
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("A valid email is required.");
    }
}