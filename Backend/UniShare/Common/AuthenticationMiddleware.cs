using System.Security.Claims;

namespace UniShare.Common;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract bearer token from Authorization header
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        var token = authHeader?.Split(" ").LastOrDefault();

        if (!string.IsNullOrEmpty(token) && token.StartsWith("temp-token-"))
        {
            var userIdString = token.Replace("temp-token-", "");
            if (Guid.TryParse(userIdString, out var userId))
            {
                // Add userId to HttpContext.Items so it can be accessed in handlers
                context.Items["UserId"] = userId;
            }
        }

        await _next(context);
    }
}

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
