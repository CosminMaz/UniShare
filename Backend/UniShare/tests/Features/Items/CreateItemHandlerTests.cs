using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniShare.Infrastructure.Features.Items.CreateItem;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;
using UniShare.Common;

namespace UniShare.tests.Features.Items;

public class CreateItemHandlerTests : IDisposable
{
    private readonly UniShareContext _context;
    private readonly Mock<IValidator<CreateItemRequest>> _validatorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CreateItemHandler _handler;

    public CreateItemHandlerTests()
    {
        Log.Info("Setting up CreateItemHandlerTests...");
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_CreateItem")
            .Options;
        _context = new UniShareContext(options);
        var loggerMock = new Mock<ILogger<CreateItemHandler>>();
        _validatorMock = new Mock<IValidator<CreateItemRequest>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new CreateItemHandler(_context, _validatorMock.Object, _httpContextAccessorMock.Object);
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
        var owner = new User(Guid.NewGuid(), "Owner User", "owner@example.com", "hashedpassword", Role.User, DateTime.UtcNow);
        _context.Users.Add(owner);
        await _context.SaveChangesAsync();

        var request = new CreateItemRequest(
            "Test Book",
            "A great book for studying",
            Category.Books,
            Condition.New,
            10.0m,
            "https://example.com/image.jpg"
        );

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Setup HttpContext with UserId
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = owner.Id;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var createdResult = Assert.IsType<Created<Item>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.Equal(request.Title, createdResult.Value!.Title);
        Assert.Equal(request.Description, createdResult.Value!.Description);
        Assert.Equal(owner.Id, createdResult.Value!.OwnerId);

        var itemInDb = await _context.Items.FirstOrDefaultAsync(i => i.Id == createdResult.Value!.Id);
        Assert.NotNull(itemInDb);
        Assert.Equal(request.Title, itemInDb.Title);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsValidationProblem()
    {
        // Arrange
        var request = new CreateItemRequest(
            "",
            "",
            Category.Books,
            Condition.New,
            null,
            null
        );

        var validationFailures = new List<ValidationFailure>
        {
            new("Title", "Title must be at least 3 characters long."),
            new("Description", "Description must be at least 5 characters long.")
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Setup HttpContext (even though validation will fail first)
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var validationProblemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, validationProblemResult.StatusCode);
    }

    [Fact]
    public async Task Handle_WithNullHttpContext_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateItemRequest(
            "Test Book",
            "A great book for studying",
            Category.Books,
            Condition.New,
            10.0m,
            "https://example.com/image.jpg"
        );

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Setup HttpContext as null
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Handle_WithMissingUserId_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateItemRequest(
            "Test Book",
            "A great book for studying",
            Category.Books,
            Condition.New,
            10.0m,
            "https://example.com/image.jpg"
        );

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Setup HttpContext without UserId
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Handle_WithNonExistentOwner_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentOwnerId = Guid.NewGuid();
        var request = new CreateItemRequest(
            "Test Book",
            "A great book for studying",
            Category.Books,
            Condition.New,
            10.0m,
            "https://example.com/image.jpg"
        );

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Setup HttpContext with non-existent UserId
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = nonExistentOwnerId;
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("BadRequest", result.GetType().Name);
        Assert.False(result is Created<Item>);
    }
}

