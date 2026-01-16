using UniShare.Infrastructure.Features.Bookings.CreateBooking;
using UniShare.Infrastructure.Validators;
using Xunit;

namespace UniShare.Tests.Features.Bookings;

public class CreateBookingValidatorTests
{
    private readonly CreateBookingValidator _validator = new();

    [Fact]
    public void Validate_ReturnsSuccess_ForValidPayload()
    {
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenItemIdMissing()
    {
        var request = new CreateBookingRequest(
            Guid.Empty,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "ItemId" && e.ErrorMessage.Contains("ItemId is required"));
    }

    [Fact]
    public void Validate_Fails_WhenStartDateMissing()
    {
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            default,
            DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate" && e.ErrorMessage.Contains("StartDate is required"));
    }

    [Fact]
    public void Validate_Fails_WhenEndDateMissing()
    {
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            default);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage.Contains("EndDate is required"));
    }

    [Fact]
    public void Validate_Fails_WhenEndDateBeforeStartDate()
    {
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("EndDate must be after StartDate"));
    }

    [Fact]
    public void Validate_Fails_WhenStartDateInThePast()
    {
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("StartDate cannot be in the past"));
    }
}
