using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using UniShare.Infrastructure.Features.Reviews.CreateReview;
using UniShare.Infrastructure.Features.Reviews;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;
using UniShare.Common;

namespace UniShare.tests.Features.Reviews;

public class CreateReviewHandlerTests : IDisposable
{
    private readonly UniShareContext _context;
    private readonly Mock<IValidator<CreateReviewRequest>> _validatorMock;
    private readonly CreateReviewHandler _handler;

    public CreateReviewHandlerTests()
    {
        Log.Info("Setting up CreateReviewHandlerTests...");
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_CreateReview")
            .Options;
        _context = new UniShareContext(options);
        var loggerMock = new Mock<ILogger<CreateReviewHandler>>();
        _validatorMock = new Mock<IValidator<CreateReviewRequest>>();
        _handler = new CreateReviewHandler(_context, _validatorMock.Object);
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
        var reviewer = new User(Guid.NewGuid(), "Reviewer User", "reviewer@example.com", "hashedpassword", Role.User, DateTime.UtcNow);
        var item = new Item(Guid.NewGuid(), Guid.NewGuid(), "Test Item", "Description", Category.Books, Condition.New, 10.0m, "image.jpg", true, DateTime.UtcNow);
        
        _context.Users.Add(reviewer);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var request = new CreateReviewRequest(
            Guid.NewGuid(), // BookingId
            reviewer.Id,
            item.Id,
            5,
            "Great item, highly recommend!",
            ReviewType.Good
        );
        
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        var createdResult = Assert.IsType<Created<Review>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.Equal(request.Rating, createdResult.Value!.Rating);
        Assert.Equal(request.Comment, createdResult.Value!.Comment);
        Assert.Equal(request.ReviewerId, createdResult.Value!.ReviewerId);
        Assert.Equal(request.ItemId, createdResult.Value!.ItemId);

        var reviewInDb = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == createdResult.Value!.Id);
        Assert.NotNull(reviewInDb);
        Assert.Equal(request.Rating, reviewInDb.Rating);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsValidationProblem()
    {
        // Arrange
        var request = new CreateReviewRequest(
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            0,
            "",
            null
        );
        
        var validationFailures = new List<ValidationFailure>
        {
            new("BookingId", "BookingId is required."),
            new("ReviewerId", "ReviewerId is required."),
            new("ItemId", "ItemId is required."),
            new("Rating", "Rating must be between 1 and 5."),
            new("Comment", "Comment is required."),
            new("RevType", "Review type is required.")
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
    public async Task Handle_WithNonExistentReviewer_ReturnsBadRequest()
    {
        // Arrange
        var item = new Item(Guid.NewGuid(), Guid.NewGuid(), "Test Item", "Description", Category.Books, Condition.New, 10.0m, "image.jpg", true, DateTime.UtcNow);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var request = new CreateReviewRequest(
            Guid.NewGuid(),
            Guid.NewGuid(), // Non-existent reviewer
            item.Id,
            5,
            "Great item!",
            ReviewType.Good
        );
        
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        // Results.BadRequest returns BadRequest<T> with anonymous type, so we check the type name
        Assert.NotNull(result);
        Assert.Contains("BadRequest", result.GetType().Name);
        Assert.False(result is Created<Review>);
    }

    [Fact]
    public async Task Handle_WithNonExistentItem_ReturnsBadRequest()
    {
        // Arrange
        var reviewer = new User(Guid.NewGuid(), "Reviewer User", "reviewer@example.com", "hashedpassword", Role.User, DateTime.UtcNow);
        _context.Users.Add(reviewer);
        await _context.SaveChangesAsync();

        var request = new CreateReviewRequest(
            Guid.NewGuid(),
            reviewer.Id,
            Guid.NewGuid(), // Non-existent item
            5,
            "Great item!",
            ReviewType.Good
        );
        
        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(request);

        // Assert
        // Results.BadRequest returns BadRequest<T> with anonymous type, so we check the type name
        Assert.NotNull(result);
        Assert.Contains("BadRequest", result.GetType().Name);
        Assert.False(result is Created<Review>);
    }
}

