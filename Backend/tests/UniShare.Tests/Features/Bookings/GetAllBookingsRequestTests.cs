using UniShare.Infrastructure.Features.Bookings.GetAll;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class GetAllBookingsRequestTests
{
    [Fact]
    public void Constructor_Stores_User_Id()
    {
        var userId = Guid.NewGuid();

        var request = new GettAllBookingsRequest(userId);

        Assert.Equal(userId, request.UserId);
    }
}
