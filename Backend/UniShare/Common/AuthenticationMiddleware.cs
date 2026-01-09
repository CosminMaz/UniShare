namespace UniShare.Common;

public class AuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract bearer token from Authorization header
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        var token = authHeader?.Split(" ").LastOrDefault();

        // For SignalR WebSocket connections, the token is passed in the query string.
        if (string.IsNullOrEmpty(token) && context.Request.Query.TryGetValue("access_token", out var accessToken))
        {
            token = accessToken;
        }

        if (!string.IsNullOrEmpty(token) && token.StartsWith("temp-token-"))
        {
            var userIdString = token.Replace("temp-token-", "");
            if (Guid.TryParse(userIdString, out var userId))
            {
                // Add userId to HttpContext.Items so it can be accessed in handlers
                context.Items["UserId"] = userId;
            }
        }

        await next(context);
    }
}

public static class AuthenticationMiddlewareExtensions
{
    public static void UseAuthenticationMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
