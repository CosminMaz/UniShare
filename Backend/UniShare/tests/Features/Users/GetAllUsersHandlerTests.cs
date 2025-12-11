using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Features.Users.GetAll;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Users;

public class GetAllUsersHandlerTests
{
    private static UniShareContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    [Fact]
    public async Task Handle_Returns_Ok_With_All_Users()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllUsersHandler(context);

        context.Users.Add(new User(Guid.NewGuid(), "Alice", "alice@test.com", "hashed", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(Guid.NewGuid(), "Bob", "bob@test.com", "hashed", Role.Admin, DateTime.UtcNow));
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle();

        // Assert
        var okResult = Assert.IsType<Ok<List<User>>>(result);
        var users = okResult.Value!;
        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.FullName == "Alice");
        Assert.Contains(users, u => u.FullName == "Bob");
    }

    [Fact]
    public async Task Handle_When_No_Users_Returns_Empty_List()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllUsersHandler(context);

        // Act
        var result = await handler.Handle();

        // Assert
        var okResult = Assert.IsType<Ok<List<User>>>(result);
        var users = okResult.Value!;
        Assert.Empty(users);
    }

    [Fact]
    public async Task Handle_Returns_Users_With_Different_Roles()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllUsersHandler(context);

        context.Users.Add(new User(Guid.NewGuid(), "Admin User", "admin@test.com", "hashed", Role.Admin, DateTime.UtcNow));
        context.Users.Add(new User(Guid.NewGuid(), "Regular User", "user@test.com", "hashed", Role.User, DateTime.UtcNow));
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle();

        // Assert
        var okResult = Assert.IsType<Ok<List<User>>>(result);
        var users = okResult.Value!.ToList();
        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Role == Role.Admin);
        Assert.Contains(users, u => u.Role == Role.User);
    }
}

