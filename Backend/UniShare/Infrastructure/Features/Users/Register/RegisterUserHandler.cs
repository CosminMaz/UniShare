using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

namespace UniShare.Infrastructure.Features.Users.Register;

public class RegisterUserHandler(
    UniShareContext context,
    IValidator<RegisterUserRequest> validator)
{
    public async Task<IResult> Handle(RegisterUserRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            
            Log.Error("Validation failed for RegisterUserRequest");
            return Results.ValidationProblem(errors);
        }

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser is not null)
        {
            Log.Error($"User with email: {request.Email} already exists");
            return Results.Problem("A user with this email already exists.", statusCode: 409);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(Guid.NewGuid(), request.Fullname, request.Email, passwordHash, Role.User, DateTime.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        Log.Info($"User with id: {user.Id} was registered");
        return Results.Created($"/users/{user.Id}", user);
    }
}
