using Microsoft.AspNetCore.Mvc;
using UniShare.Infrastructure.Features.Users.Login;
using UniShare.Infrastructure.Features.Users.Register;
using UniShare.Infrastructure.Features.Users;

namespace UniShare.Api;

public static class UserApi
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async ([FromServices] GetAllUsersHandler handler) => await handler.Handle())
            .WithName("GetAllUsers");

        app.MapPost("/api/auth/register", async (RegisterUserRequest request, [FromServices] RegisterUserHandler handler) => await handler.Handle(request))
            .WithName("RegisterUser");

        app.MapPost("/api/auth/login", async (LoginRequest request, [FromServices] LoginHandler handler) => await handler.Handle(request))
            .WithName("LoginUser");
    }
}
