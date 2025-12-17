using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Features.Users.Login;
using UniShare.Infrastructure.Features.Users.Register;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Api;

public class UserApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public UserApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true
        };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // Clean up the database before each test
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAllUsers_WhenNoUsersExist_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<User>>(_jsonSerializerOptions);
        Assert.NotNull(users);
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetAllUsers_WhenUsersExist_ReturnsUsers()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        context.Users.Add(new User(Guid.NewGuid(), "Test User 1", "test1@test.com", "hash", Role.User, DateTime.UtcNow));
        context.Users.Add(new User(Guid.NewGuid(), "Test User 2", "test2@test.com", "hash", Role.Admin, DateTime.UtcNow));
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/users");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<User>>(_jsonSerializerOptions);
        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new RegisterUserRequest("New User", "new@test.com", "Password123!");
        var content = new StringContent(JsonSerializer.Serialize(request, _jsonSerializerOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task LoginUser_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        var password = "password";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        context.Users.Add(new User(Guid.NewGuid(), "Test User", "test@test.com", hashedPassword, Role.User, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var request = new LoginRequest("test@test.com", password);
        var content = new StringContent(JsonSerializer.Serialize(request, _jsonSerializerOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonSerializerOptions);
        Assert.NotNull(loginResponse);
        Assert.NotNull(loginResponse.Token);
        Assert.Equal("Test User", loginResponse.User.FullName);
    }
    
    [Fact]
    public async Task LoginUser_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nouser@test.com", "wrongpassword");
        var content = new StringContent(JsonSerializer.Serialize(request, _jsonSerializerOptions), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
