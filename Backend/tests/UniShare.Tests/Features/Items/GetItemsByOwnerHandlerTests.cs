using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Items.GetByOwner;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Features.Items;

public class GetItemsByOwnerHandlerTests
{
    private static UniShareContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UniShareContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UniShareContext(options);
    }

    [Fact]
    public async Task Handle_Returns_Items_For_Owner_In_Descending_Order()
    {
        var ownerId = Guid.NewGuid();
        var otherOwner = Guid.NewGuid();

        var context = CreateContext();
        context.Items.Add(new Item(Guid.NewGuid(), ownerId, "Old", "Old desc", Category.Other, Condition.Acceptable, 1m, "img", true, DateTime.UtcNow.AddDays(-2)));
        context.Items.Add(new Item(Guid.NewGuid(), ownerId, "New", "New desc", Category.Books, Condition.New, 2m, "img", true, DateTime.UtcNow));
        context.Items.Add(new Item(Guid.NewGuid(), otherOwner, "Foreign", "Foreign desc", Category.Electronics, Condition.LikeNew, 3m, "img", true, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var handler = new GetItemsByOwner.Handler(context);

        var result = (await handler.Handle(new GetItemsByOwner.Query(ownerId), CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal(ownerId, item.OwnerId));
        Assert.Equal("New", result[0].Title);
        Assert.Equal("Old", result[1].Title);
    }
}
