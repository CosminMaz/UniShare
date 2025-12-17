using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Items.CreateItem;
using UniShare.Infrastructure.Features.Items.Delete;
using UniShare.Infrastructure.Features.Items.GetAll;
using UniShare.Infrastructure.Features.Items.GetByOwner;

namespace UniShare.Api;

public static class ItemApi
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/items", async ([FromBody] CreateItemRequest request, [FromServices] CreateItemHandler handler) => await handler.Handle(request))
            .WithName("CreateItem")
            .Accepts<CreateItemRequest>("application/json")
            .Produces(201)
            .Produces(400)
            .Produces(401);

        app.MapGet("/items", async (IMediator mediator) => await mediator.Send(new GetAllItems.Query()))
            .WithName("GetAllItems");

        app.MapGet("/items/mine", async (IMediator mediator, IHttpContextAccessor httpContextAccessor) =>
            {
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext is null || !httpContext.Items.TryGetValue("UserId", out var userIdObj) || userIdObj is not Guid userId)
                {
                    return Results.Unauthorized();
                }

                var items = await mediator.Send(new GetItemsByOwner.Query(userId));
                return Results.Ok(items);
            })
            .WithName("GetMyItems")
            .Produces<IEnumerable<Item>>(200)
            .Produces(401);

        app.MapDelete("/items/{id:guid}", async (Guid id, [FromServices] DeleteItemHandler handler) =>
                await handler.Handle(id))
            .WithName("DeleteItem")
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404);
    }
}
