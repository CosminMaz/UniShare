using FluentValidation;
using UniShare.Infrastructure.Features.Users.Register;
namespace UniShare.Infrastructure.Validators;

public class CreateUserValidator:AbstractValidator<RegisterUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x=> x.Fullname).NotNull().NotEmpty().MinimumLength(3).WithMessage("FullName must be at least 3 characters long.");
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("A valid email is required.");
        RuleFor(x => x.Password)
            .NotNull().NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}