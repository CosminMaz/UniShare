using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.tests.Features.Users;

public class LoginHandlerTests : IDisposable
{
    private readonly UniShareContext _context;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new UniShareContext(options);
        var loggerMock = new Mock<ILogger<LoginHandler>>();
        _handler = new LoginHandler(_context, loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsOkResultWithTokenAndUserDto()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User(Guid.NewGuid(), "Test User", "test@example.com", hashedPassword, Role.User, DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("test@example.com", password);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var okResult = Assert.IsType<Ok<LoginResponse>>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        Assert.Equal($"temp-token-{user.Id}", response.Token);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal(user.Email, response.User.Email);
        Assert.Equal(user.FullName, response.User.FullName);
        Assert.Equal(user.Role.ToString(), response.User.Role);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "password");

        // Act
        var result = await _handler.Handle(request);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsUnauthorizedResult()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User(Guid.NewGuid(), "Test User", "test@example.com", hashedPassword, Role.User, DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("test@example.com", "wrongpassword");

        // Act
        var result = await _handler.Handle(request);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }
}
