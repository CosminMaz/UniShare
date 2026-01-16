using System.Linq;
using UniShare.Infrastructure.Features.Items.GetAll;
using Xunit;

namespace UniShare.Tests.Features.Items;

public class GetAllItemsRequestTests
{
    [Fact]
    public void Constructor_Stores_OwnerId()
    {
        var ownerId = Guid.NewGuid();

        var request = new GetAllItemsRequest(ownerId);

        Assert.NotNull(request);
        var parameterName = typeof(GetAllItemsRequest)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Single()
            .Name;

        Assert.Equal("OwnerId", parameterName);
    }
}
