using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Users.Login;
public class LoginHandler(UniShareContext context)
{
    public async Task<IResult> Handle(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        // TODO: Replace with real JWT generation.
        var token = $"temp-token-{user.Id}";

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString());
        return Results.Ok(new LoginResponse(token, userDto));
    }
}
