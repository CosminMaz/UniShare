using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Reviews;
using UniShare.Infrastructure.Features.Reviews.GetAll;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Reviews;

public class GetAllReviewsHandlerTests
{
    private static UniShareContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    [Fact]
    public async Task Handle_Returns_All_Reviews()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllReviews.Handler(context);

        var reviewerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var bookingId1 = Guid.NewGuid();
        var bookingId2 = Guid.NewGuid();

        context.Reviews.Add(new Review(Guid.NewGuid(), bookingId1, reviewerId, itemId, 5, "Excellent!", ReviewType.VeryGood, DateTime.UtcNow));
        context.Reviews.Add(new Review(Guid.NewGuid(), bookingId2, reviewerId, itemId, 3, "Okay", ReviewType.Ok, DateTime.UtcNow));
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetAllReviews.Query(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Rating == 5);
        Assert.Contains(result, r => r.Rating == 3);
        Assert.Contains(result, r => r.RevType == ReviewType.VeryGood);
        Assert.Contains(result, r => r.RevType == ReviewType.Ok);
    }

    [Fact]
    public async Task Handle_When_No_Reviews_Returns_Empty()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllReviews.Handler(context);

        // Act
        var result = await handler.Handle(new GetAllReviews.Query(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Returns_Reviews_With_Different_Types()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllReviews.Handler(context);

        var reviewerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Reviews.Add(new Review(Guid.NewGuid(), Guid.NewGuid(), reviewerId, itemId, 1, "Terrible", ReviewType.Bad, DateTime.UtcNow));
        context.Reviews.Add(new Review(Guid.NewGuid(), Guid.NewGuid(), reviewerId, itemId, 2, "Not great", ReviewType.Ok, DateTime.UtcNow));
        context.Reviews.Add(new Review(Guid.NewGuid(), Guid.NewGuid(), reviewerId, itemId, 4, "Good", ReviewType.Good, DateTime.UtcNow));
        context.Reviews.Add(new Review(Guid.NewGuid(), Guid.NewGuid(), reviewerId, itemId, 5, "Perfect", ReviewType.VeryGood, DateTime.UtcNow));
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetAllReviews.Query(), CancellationToken.None);

        // Assert
        Assert.Equal(4, result.Count());
        Assert.Contains(result, r => r.RevType == ReviewType.Bad);
        Assert.Contains(result, r => r.RevType == ReviewType.Ok);
        Assert.Contains(result, r => r.RevType == ReviewType.Good);
        Assert.Contains(result, r => r.RevType == ReviewType.VeryGood);
    }
}

