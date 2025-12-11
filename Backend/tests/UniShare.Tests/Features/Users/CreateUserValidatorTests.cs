using FluentValidation.TestHelper;
using UniShare.Infrastructure.Features.Users.Register;
using UniShare.Infrastructure.Validators;
using Xunit;

namespace UniShare.tests.Features.Users;

public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator;

    public CreateUserValidatorTests()
    {
        _validator = new CreateUserValidator();
    }

    [Fact]
    public void Should_have_error_when_fullname_is_empty()
    {
        var request = new RegisterUserRequest("", "test@example.com", "Password123!");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Fullname);
    }

    [Fact]
    public void Should_not_have_error_when_fullname_is_specified()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "Password123!");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Fullname);
    }

    [Fact]
    public void Should_have_error_when_email_is_empty()
    {
        var request = new RegisterUserRequest("Test User", "", "Password123!");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_have_error_when_email_is_invalid_format()
    {
        var request = new RegisterUserRequest("Test User", "invalid-email", "Password123!");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_not_have_error_when_email_is_valid_format()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "Password123!");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_have_error_when_password_is_empty()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_have_error_when_password_is_too_short()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "short");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_have_error_when_password_does_not_contain_uppercase()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "password123!");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_have_error_when_password_does_not_contain_lowercase()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "PASSWORD123!");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_have_error_when_password_does_not_contain_digit()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "Password!");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_have_error_when_password_does_not_contain_special_character()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "Password123");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [Fact]
    public void Should_not_have_error_when_password_is_strong()
    {
        var request = new RegisterUserRequest("Test User", "test@example.com", "Password123!");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
