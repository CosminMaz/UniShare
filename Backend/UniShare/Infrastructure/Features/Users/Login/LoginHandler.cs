using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

namespace UniShare.Infrastructure.Features.Users.Login;
public class LoginHandler(UniShareContext context)
{
    public async Task<IResult> Handle(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            Log.Error($"Invalid credentials for user with email: {request.Email}");
            return Results.Unauthorized();
        }

        // TODO: Replace with real JWT generation.
        var token = $"temp-token-{user.Id}";

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString());
        Log.Info($"User with id: {user.Id} logged in");
        return Results.Ok(new LoginResponse(token, userDto));
    }
}
