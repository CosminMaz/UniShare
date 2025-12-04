using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniShare.Infrastructure.Features.Reviews.CreateReview;
using UniShare.Infrastructure.Features.Reviews.GetAll;

namespace UniShare.Api;

public static class ReviewApi
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/reviews", async (CreateReviewRequest request, [FromServices] CreateReviewHandler handler) =>
            await handler.Handle(request))
            .WithName("CreateReview");

        app.MapGet("/reviews", async (IMediator mediator) =>
                await mediator.Send(new GetAllReviews.Query()))
            .WithName("GetAllReviews");
    }
}