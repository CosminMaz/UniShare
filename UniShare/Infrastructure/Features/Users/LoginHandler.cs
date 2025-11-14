using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace UniShare.Infrastructure.Features.Users;

public class LoginHandler
{
    private readonly UniShareContext _context;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(UniShareContext context, ILogger<LoginHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IResult> Handle(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            Log.Warning($"Failed login attempt for {request.Email}");
            return Results.Unauthorized();
        }

        // TODO: Replace with real JWT generation.
        var token = $"temp-token-{user.Id}";

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString());

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
        Log.Info($"User {user.Email} connected at {DateTime.UtcNow}");

        return Results.Ok(new LoginResponse(token, userDto));
    }
}
