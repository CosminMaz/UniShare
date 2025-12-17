using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Items.GetAll;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Items;

public class GetAllItemsHandlerTests
{
    private static UniShareContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    [Fact]
    public async Task Handle_Returns_All_Items()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllItems.Handler(context);
        var ownerId = Guid.NewGuid();

        context.Items.Add(new Item(Guid.NewGuid(), ownerId, "Book", "Nice book", Category.Books, Condition.New, 5m, null, true, DateTime.UtcNow));
        context.Items.Add(new Item(Guid.NewGuid(), ownerId, "Camera", "DSLR camera", Category.Electronics, Condition.LikeNew, 25m, null, false, DateTime.UtcNow));
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new GetAllItems.Query(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, i => i.Title == "Book");
        Assert.Contains(result, i => i.Title == "Camera");
    }

    [Fact]
    public async Task Handle_When_No_Items_Returns_Empty()
    {
        // Arrange
        var context = CreateContext();
        var handler = new GetAllItems.Handler(context);

        // Act
        var result = await handler.Handle(new GetAllItems.Query(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}

