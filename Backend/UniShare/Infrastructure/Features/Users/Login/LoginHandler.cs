using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

namespace UniShare.Infrastructure.Features.Users.Login;
public class LoginHandler(
    UniShareContext context,
    IValidator<LoginRequest> validator)
{
    public async Task<IResult> Handle(LoginRequest request)
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

            Log.Error("Validation failed for LoginRequest");
            return Results.ValidationProblem(errors);
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            Log.Error($"Invalid credentials for user with email: {request.Email}");
            return Results.Unauthorized();
        }

        var token = $"temp-token-{user.Id}";

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString());
        Log.Info($"User with id: {user.Id} logged in");
        return Results.Ok(new LoginResponse(token, userDto));
    }
}
