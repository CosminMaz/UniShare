using UniShare.Infrastructure.Features.Reviews.GetAll;
using Xunit;

namespace UniShare.Tests.Features.Reviews;

public class GetAllReviewsRequestTests
{
    [Fact]
    public void Constructor_Stores_Item_Id()
    {
        var itemId = Guid.NewGuid();

        var request = new GetAllReviewsRequest(itemId);

        Assert.Equal(itemId, request.Item);
    }
}
