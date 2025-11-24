using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Users.Login;
public class LoginHandler(UniShareContext context, ILogger<LoginHandler> logger)
{
    public async Task<IResult> Handle(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return Results.Unauthorized();
        }

        // TODO: Replace with real JWT generation.
        var token = $"temp-token-{user.Id}";

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString());

        logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Results.Ok(new LoginResponse(token, userDto));
    }
}
