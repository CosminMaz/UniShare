using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Users;

public class CreateUserHandler(
    UniShareContext context,
    ILogger<CreateUserHandler> logger,
    IValidator<CreateUserRequest> validator)
{
    public async Task<IResult> Handle(CreateUserRequest request)
    {
        logger.LogInformation("Creating new user with Name: {Fullname} and Email: {Email}", request.Fullname, request.Email);

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return Results.ValidationProblem(errors);
        }

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser is not null)
        {
            return Results.Problem("A user with this email already exists.", statusCode: 409);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(Guid.NewGuid(), request.Fullname, request.Email, passwordHash, Role.User, DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

        return Results.Created($"/users/{user.Id}", user);
    }
}
