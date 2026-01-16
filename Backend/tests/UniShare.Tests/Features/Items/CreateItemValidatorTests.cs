using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Items.CreateItem;
using UniShare.Infrastructure.Validators;
using Xunit;

namespace UniShare.Tests.Features.Items;

public class CreateItemValidatorTests
{
    private readonly CreateItemValidator _validator = new();

    [Fact]
    public void Validate_ReturnsSuccess_ForValidPayload()
    {
        var request = new CreateItemRequest(
            "Camera",
            "DSLR camera in good condition",
            Category.Electronics,
            Condition.LikeNew,
            25m,
            "http://example.com/camera.jpg");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_For_Short_Title_And_Description()
    {
        var request = new CreateItemRequest(
            "Ca",
            "desc",
            Category.Books,
            Condition.New,
            5m,
            "img");

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage.Contains("Title must be at least 3 characters long."));
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage.Contains("Description must be at least 5 characters long."));
    }

    [Fact]
    public void Validate_Fails_For_Invalid_Enums_And_DailyRate()
    {
        var request = new CreateItemRequest(
            "Valid title",
            "Valid description",
            (Category)999,
            (Condition)999,
            -1m,
            "img");

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "Categ");
        Assert.Contains(result.Errors, e => e.PropertyName == "Cond");
        Assert.Contains(result.Errors, e => e.PropertyName == "DailyRate" && e.ErrorMessage.Contains("positive"));
    }

    [Fact]
    public void Validate_Fails_When_ImageUrlMissing()
    {
        var request = new CreateItemRequest(
            "Valid title",
            "Valid description",
            Category.Other,
            Condition.Acceptable,
            null,
            string.Empty);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "ImageUrl" && e.ErrorMessage.Contains("required"));
    }
}
