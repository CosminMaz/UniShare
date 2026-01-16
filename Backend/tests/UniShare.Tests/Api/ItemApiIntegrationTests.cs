using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Items.CreateItem;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Api;

public class ItemApiIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ItemApiIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            Converters = { new JsonStringEnumConverter() }
        };

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Create_And_Get_My_Items_Work_EndToEnd()
    {
        var ownerId = Guid.NewGuid();
        await SeedUserAsync(ownerId);

        var client = CreateAuthorizedClient(ownerId);
        var createRequest = new CreateItemRequest(
            "Laptop",
            "Lightweight laptop",
            Category.Electronics,
            Condition.LikeNew,
            30m,
            "http://example.com/laptop.jpg");

        var createResponse = await client.PostAsJsonAsync("/items", createRequest, _jsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);

        var allItemsResponse = await client.GetAsync("/items");
        allItemsResponse.EnsureSuccessStatusCode();
        var allItems = await allItemsResponse.Content.ReadFromJsonAsync<List<Item>>(_jsonOptions);
        Assert.NotNull(allItems);
        Assert.Single(allItems);

        var myItemsResponse = await client.GetAsync("/items/mine");
        myItemsResponse.EnsureSuccessStatusCode();
        var myItems = await myItemsResponse.Content.ReadFromJsonAsync<List<Item>>(_jsonOptions);
        Assert.NotNull(myItems);
        Assert.Single(myItems);
        Assert.Equal(ownerId, myItems[0].OwnerId);
    }

    [Fact]
    public async Task DeleteItem_Removes_Item_For_Owner()
    {
        var ownerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await SeedUserAsync(ownerId);
        await SeedItemAsync(itemId, ownerId);

        var client = CreateAuthorizedClient(ownerId);
        var deleteResponse = await client.DeleteAsync($"/items/{itemId}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var allItemsResponse = await client.GetAsync("/items");
        allItemsResponse.EnsureSuccessStatusCode();
        var allItems = await allItemsResponse.Content.ReadFromJsonAsync<List<Item>>(_jsonOptions);
        Assert.NotNull(allItems);
        Assert.Empty(allItems);
    }

    [Fact]
    public async Task GetMyItems_Returns_Unauthorized_When_Token_Missing()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/items/mine");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateAuthorizedClient(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer temp-token-{userId}");
        return client;
    }

    private async Task SeedUserAsync(Guid userId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        context.Users.Add(new Infrastructure.Features.Users.User(userId, "Owner", "owner@test.com", "hash", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        await context.SaveChangesAsync();
    }

    private async Task SeedItemAsync(Guid itemId, Guid ownerId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        context.Items.Add(new Item(itemId, ownerId, "Old Laptop", "Old desc", Category.Electronics, Condition.WellPreserved, 12m, "img", true, DateTime.UtcNow));
        await context.SaveChangesAsync();
    }
}
