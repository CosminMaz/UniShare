using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;
using UniShare.Common;

namespace UniShare.tests.Features.Users;

public class CreateUserHandlerTests : IDisposable
{
    private readonly UniShareContext _context;
    private readonly Mock<IValidator<CreateUserRequest>> _validatorMock;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        Log.Info("Setting up CreateUserHandlerTests...");
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_CreateUser")
            .Options;
        _context = new UniShareContext(options);
        var loggerMock = new Mock<ILogger<CreateUserHandler>>();
        _validatorMock = new Mock<IValidator<CreateUserRequest>>();
        _handler = new CreateUserHandler(_context, loggerMock.Object, _validatorMock.Object);
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
        var request = new CreateUserRequest("Test User", "test@example.com", "password123");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var createdResult = Assert.IsType<Created<User>>(result);
        Assert.Equal(request.Fullname, createdResult.Value.FullName);
        Assert.Equal(request.Email, createdResult.Value.Email);

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(userInDb);
        Assert.Equal(request.Fullname, userInDb.FullName);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsValidationProblem()
    {
        // Arrange
        var request = new CreateUserRequest("", "", "");
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

        var request = new CreateUserRequest("New User", "test@example.com", "password123");
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var validationProblemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, validationProblemResult.StatusCode);
    }
}
