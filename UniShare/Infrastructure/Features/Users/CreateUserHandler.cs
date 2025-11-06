using FluentValidation;
using Microsoft.VisualBasic.CompilerServices;
using UniShare.Infrastructure.Persistence;
namespace UniShare.Infrastructure.Features.Users;

public class CreateUserHandler
{
    private readonly UniShareContext _context;
    private readonly ILogger<CreateUserHandler> _logger;
    private readonly IValidator<CreateUserRequest> _validator;

    public CreateUserHandler(UniShareContext context, ILogger<CreateUserHandler> logger, IValidator<CreateUserRequest> validator)
    {
        _context = context;
        _logger = logger;
        _validator = validator;
    }

    public async Task<IResult> Handle(CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user with Name: {Fullname} and Email: {Email}", request.Fullname, request.Email);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var user = new User(Guid.NewGuid(), request.Fullname, request.Email, Role.User, DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

        return Results.Created($"/users/{user.Id}", user);
    }
}
