using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UniShare.Infrastructure.Features.Users.Register;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;
using UniShare.Common;

namespace UniShare.tests.Features.Users;

public class CreateUserHandlerTests : IDisposable
{
    private readonly UniShareContext _context;
    private readonly Mock<IValidator<RegisterUserRequest>> _validatorMock;
    private readonly RegisterUserHandler _handler;

    public CreateUserHandlerTests()
    {
        Log.Info("Setting up CreateUserHandlerTests...");
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_CreateUser")
            .Options;
        _context = new UniShareContext(options);
        var loggerMock = new Mock<ILogger<RegisterUserHandler>>();
        _validatorMock = new Mock<IValidator<RegisterUserRequest>>();
        _handler = new RegisterUserHandler(_context, _validatorMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterUserRequest("Test User", "test@example.com", "password123");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var createdResult = Assert.IsType<Created<User>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.Equal(request.Fullname, createdResult.Value!.FullName);
        Assert.Equal(request.Email, createdResult.Value!.Email);

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(userInDb);
        Assert.Equal(request.Fullname, userInDb.FullName);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsValidationProblem()
    {
        // Arrange
        var request = new RegisterUserRequest("", "", "");
        var validationFailures = new List<ValidationFailure>
        {
            new("Fullname", "Fullname is required."),
            new("Email", "Email is not in a valid format.")
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
    public async Task Handle_WithExistingEmail_ReturnsValidationProblem()
    {
        // Arrange
        var existingUser = new User(Guid.NewGuid(), "Existing User", "test@example.com", "hashedpassword", Role.User, DateTime.UtcNow);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterUserRequest("New User", "test@example.com", "password123");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var validationProblemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, validationProblemResult.StatusCode);
    }

    [Fact]
    public async Task Handle_Password_Is_Hashed()
    {
        // Arrange
        var password = "password123";
        var request = new RegisterUserRequest("Test User", "test@example.com", password);
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var createdResult = Assert.IsType<Created<User>>(result);
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(userInDb);
        Assert.NotEqual(password, userInDb.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(password, userInDb.PasswordHash));
    }

    [Fact]
    public async Task Handle_User_Is_Created_With_User_Role()
    {
        // Arrange
        var request = new RegisterUserRequest("Test User", "test@example.com", "password123");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var createdResult = Assert.IsType<Created<User>>(result);
        Assert.Equal(Role.User, createdResult.Value!.Role);
        
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(userInDb);
        Assert.Equal(Role.User, userInDb.Role);
    }

    [Fact]
    public async Task Handle_User_Has_CreatedAt_Timestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var request = new RegisterUserRequest("Test User", "test@example.com", "password123");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);
        var afterCreation = DateTime.UtcNow;

        // Assert
        var createdResult = Assert.IsType<Created<User>>(result);
        Assert.True(createdResult.Value!.CreatedAt >= beforeCreation);
        Assert.True(createdResult.Value!.CreatedAt <= afterCreation);
        
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(userInDb);
        Assert.True(userInDb.CreatedAt >= beforeCreation);
        Assert.True(userInDb.CreatedAt <= afterCreation);
    }

    [Fact]
    public async Task Handle_CaseSensitive_Email_Check()
    {
        // Arrange
        var existingUser = new User(Guid.NewGuid(), "Existing User", "Test@Example.com", "hashedpassword", Role.User, DateTime.UtcNow);
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Try to register with same email but different case
        var request = new RegisterUserRequest("New User", "test@example.com", "password123");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        // Note: This test depends on how the database handles case sensitivity
        // PostgreSQL by default is case-sensitive, so this should succeed
        // If using a case-insensitive collation, this would fail
        var createdResult = Assert.IsType<Created<User>>(result);
        Assert.NotNull(createdResult.Value);
    }
}
