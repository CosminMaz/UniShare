using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Bookings.ApproveBooking;
using UniShare.Infrastructure.Features.Bookings.CreateBooking;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Reviews;
using UniShare.Infrastructure.Features.Reviews.CreateReview;
using UniShare.Infrastructure.Persistence;
using Xunit;

namespace UniShare.Tests.Api;

public class BookingApiIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public BookingApiIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task Booking_Lifecycle_Works_Through_Api()
    {
        var ownerId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await SeedUsersAndItemAsync(ownerId, borrowerId, itemId);

        var borrowerClient = CreateAuthorizedClient(borrowerId);
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2);
        var bookingRequest = new CreateBookingRequest(itemId, startDate, endDate);

        var bookingResponse = await borrowerClient.PostAsJsonAsync("/bookings", bookingRequest, _jsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.Created, bookingResponse.StatusCode);
        var createdBooking = await bookingResponse.Content.ReadFromJsonAsync<Booking>(_jsonOptions);
        Assert.NotNull(createdBooking);
        var bookingId = createdBooking!.Id;

        var ownerClient = CreateAuthorizedClient(ownerId);
        var approveResponse = await ownerClient.PostAsJsonAsync($"/bookings/{bookingId}/approve", new ApproveBookingBody(true), _jsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.OK, approveResponse.StatusCode);

        var completeResponse = await ownerClient.PostAsync($"/bookings/{bookingId}/complete", null);
        Assert.Equal(System.Net.HttpStatusCode.OK, completeResponse.StatusCode);

        var bookingsResponse = await ownerClient.GetAsync("/bookings");
        bookingsResponse.EnsureSuccessStatusCode();
        var bookings = await bookingsResponse.Content.ReadFromJsonAsync<List<Booking>>(_jsonOptions);
        Assert.NotNull(bookings);
        Assert.Single(bookings);
        Assert.Equal(Status.Completed, bookings[0].Status);

        var reviewRequest = new CreateReviewRequest(bookingId, borrowerId, itemId, 5, "Great experience", ReviewType.VeryGood);
        var reviewResponse = await borrowerClient.PostAsJsonAsync("/reviews", reviewRequest, _jsonOptions);
        Assert.Equal(System.Net.HttpStatusCode.Created, reviewResponse.StatusCode);

        var reviewsResponse = await borrowerClient.GetAsync("/reviews");
        reviewsResponse.EnsureSuccessStatusCode();
        var reviews = await reviewsResponse.Content.ReadFromJsonAsync<List<Review>>(_jsonOptions);
        Assert.NotNull(reviews);
        Assert.Single(reviews);
        Assert.Equal("Great experience", reviews[0].Comment);
    }

    private HttpClient CreateAuthorizedClient(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer temp-token-{userId}");
        return client;
    }

    private async Task SeedUsersAndItemAsync(Guid ownerId, Guid borrowerId, Guid itemId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        context.Users.Add(new Infrastructure.Features.Users.User(ownerId, "Owner", "owner@test.com", "hash", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Users.Add(new Infrastructure.Features.Users.User(borrowerId, "Borrower", "borrower@test.com", "hash", Infrastructure.Features.Users.Role.User, DateTime.UtcNow));
        context.Items.Add(new Item(itemId, ownerId, "Bike", "Nice bike", Category.Sports, Condition.WellPreserved, 12m, "img", true, DateTime.UtcNow));
        await context.SaveChangesAsync();
    }
}
