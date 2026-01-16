using Microsoft.AspNetCore.Http;
using UniShare.Common;
using Xunit;

namespace UniShare.Tests.Common;

public class AuthenticationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Adds_UserId_When_Token_Is_Valid()
    {
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer temp-token-{userId}";

        var invoked = false;
        RequestDelegate next = _ =>
        {
            invoked = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthenticationMiddleware(next);

        await middleware.InvokeAsync(context);

        Assert.True(invoked);
        Assert.True(context.Items.TryGetValue("UserId", out var value));
        Assert.Equal(userId, value);
    }

    [Fact]
    public async Task InvokeAsync_Ignores_Request_When_Header_Missing()
    {
        var context = new DefaultHttpContext();
        var invoked = false;

        RequestDelegate next = _ =>
        {
            invoked = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthenticationMiddleware(next);

        await middleware.InvokeAsync(context);

        Assert.True(invoked);
        Assert.False(context.Items.ContainsKey("UserId"));
    }

    [Fact]
    public async Task InvokeAsync_Does_Not_Set_User_For_Invalid_Token()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer temp-token-not-a-guid";

        var middleware = new AuthenticationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.False(context.Items.ContainsKey("UserId"));
    }
}
