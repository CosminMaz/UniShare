using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniShare.Infrastructure.Features.Items.CreateItem;
using UniShare.Infrastructure.Features.Items.Delete;
using UniShare.Infrastructure.Features.Items.GetAll;

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

        app.MapDelete("/items/{id:guid}", async (Guid id, [FromServices] DeleteItemHandler handler) =>
                await handler.Handle(id))
            .WithName("DeleteItem")
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404);
    }
}
