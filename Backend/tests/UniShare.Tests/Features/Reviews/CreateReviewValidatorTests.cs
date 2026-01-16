using UniShare.Infrastructure.Features.Reviews;
using UniShare.Infrastructure.Features.Reviews.CreateReview;
using UniShare.Infrastructure.Validators;
using Xunit;

namespace UniShare.Tests.Features.Reviews;

public class CreateReviewValidatorTests
{
    private readonly CreateReviewValidator _validator = new();

    [Fact]
    public void Validate_ReturnsSuccess_ForValidRequest()
    {
        var request = new CreateReviewRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            4,
            "Solid experience",
            ReviewType.Good);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_For_Missing_Ids_And_Comment()
    {
        var request = new CreateReviewRequest(
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            3,
            string.Empty,
            null);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "BookingId");
        Assert.Contains(result.Errors, e => e.PropertyName == "ReviewerId");
        Assert.Contains(result.Errors, e => e.PropertyName == "ItemId");
        Assert.Contains(result.Errors, e => e.PropertyName == "Comment");
        Assert.Contains(result.Errors, e => e.PropertyName == "RevType");
    }

    [Fact]
    public void Validate_Fails_For_Rating_Out_Of_Range()
    {
        var tooLow = new CreateReviewRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            "Bad",
            ReviewType.Bad);

        var tooHigh = new CreateReviewRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            6,
            "Too good",
            ReviewType.VeryGood);

        Assert.Contains(_validator.Validate(tooLow).Errors, e => e.PropertyName == "Rating");
        Assert.Contains(_validator.Validate(tooHigh).Errors, e => e.PropertyName == "Rating");
    }

    [Fact]
    public void Validate_Fails_For_Too_Long_Comment()
    {
        var longComment = new string('a', 501);
        var request = new CreateReviewRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            4,
            longComment,
            ReviewType.Ok);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "Comment" && e.ErrorMessage.Contains("at most 500 characters"));
    }
}
