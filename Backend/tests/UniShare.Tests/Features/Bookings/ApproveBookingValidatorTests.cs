using FluentValidation.TestHelper;
using UniShare.Infrastructure.Features.Bookings.ApproveBooking;
using UniShare.Infrastructure.Validators;
using Xunit;
using System;

namespace UniShare.tests.Features.Bookings;

public class ApproveBookingValidatorTests
{
    private readonly ApproveBookingValidator _validator;

    public ApproveBookingValidatorTests()
    {
        _validator = new ApproveBookingValidator();
    }

    [Fact]
    public void Should_have_error_when_bookingId_is_empty()
    {
        var request = new ApproveBookingRequest(Guid.Empty, true);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BookingId);
    }

    [Fact]
    public void Should_not_have_error_when_bookingId_is_valid()
    {
        var request = new ApproveBookingRequest(Guid.NewGuid(), true);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.BookingId);
    }
}
