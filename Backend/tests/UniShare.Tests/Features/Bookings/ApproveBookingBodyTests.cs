using UniShare.Infrastructure.Features.Bookings.ApproveBooking;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class ApproveBookingBodyTests
{
    [Fact]
    public void ApproveBookingBody_Persists_Value()
    {
        var body = new ApproveBookingBody(true);

        Assert.True(body.Approve);
    }
}
