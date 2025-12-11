using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniShare.Infrastructure.Features.Users.Login;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.tests.Features.Users;

public class LoginHandlerTests : IDisposable
{
    private readonly UniShareContext _context;
    private readonly Mock<IValidator<LoginRequest>> _validatorMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new UniShareContext(options);
        _validatorMock = new Mock<IValidator<LoginRequest>>();
        _handler = new LoginHandler(_context, _validatorMock.Object);
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
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var okResult = Assert.IsType<Ok<LoginResponse>>(result);
        Assert.NotNull(okResult.Value);
        var response = okResult.Value!;
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
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

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
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsValidationProblem()
    {
        // Arrange
        var request = new LoginRequest("", "");
        var validationFailures = new List<ValidationFailure>
        {
            new("Email", "A valid email is required."),
            new("Password", "Password is required.")
        };
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var validationProblemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, validationProblemResult.StatusCode);
    }

    [Fact]
    public async Task Handle_WithEmptyPassword_ReturnsValidationProblem()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "");
        var validationFailures = new List<ValidationFailure>
        {
            new("Password", "Password is required.")
        };
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var validationProblemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, validationProblemResult.StatusCode);
    }

    [Fact]
    public async Task Handle_WithAdminRole_ReturnsCorrectRole()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User(Guid.NewGuid(), "Admin User", "admin@example.com", hashedPassword, Role.Admin, DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("admin@example.com", password);
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var okResult = Assert.IsType<Ok<LoginResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("Admin", okResult.Value!.User.Role);
    }

    [Fact]
    public async Task Handle_Token_Format_Is_Correct()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var userId = Guid.NewGuid();
        var user = new User(userId, "Test User", "test@example.com", hashedPassword, Role.User, DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest("test@example.com", password);
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var okResult = Assert.IsType<Ok<LoginResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal($"temp-token-{userId}", okResult.Value!.Token);
        Assert.StartsWith("temp-token-", okResult.Value!.Token);
    }

    [Fact]
    public async Task Handle_Email_Case_Insensitive_Login()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User(Guid.NewGuid(), "Test User", "Test@Example.com", hashedPassword, Role.User, DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Try login with different case email
        var request = new LoginRequest("test@example.com", password);
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        // Note: This depends on database collation. PostgreSQL is case-sensitive by default,
        // so this should fail. If using case-insensitive collation, it would succeed.
        // For now, we expect it to fail since PostgreSQL is case-sensitive
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Handle_UserDto_Contains_All_Fields()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var userId = Guid.NewGuid();
        var fullName = "John Doe";
        var email = "john@example.com";
        var role = Role.User;
        var user = new User(userId, fullName, email, hashedPassword, role, DateTime.UtcNow);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest(email, password);
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var okResult = Assert.IsType<Ok<LoginResponse>>(result);
        Assert.NotNull(okResult.Value);
        var userDto = okResult.Value!.User;
        Assert.Equal(userId, userDto.Id);
        Assert.Equal(fullName, userDto.FullName);
        Assert.Equal(email, userDto.Email);
        Assert.Equal(role.ToString(), userDto.Role);
    }
}
