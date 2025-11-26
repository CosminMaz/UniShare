using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Items.CreateItem;
using UniShare.Infrastructure.Features.Items.GetAll;

namespace UniShare.Api;

public static class ItemApi
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/items", async (CreateItemRequest request, [FromServices] CreateItemHandler handler) => await handler.Handle(request))
            .WithName("CreateItem");

        app.MapGet("/items", async (IMediator mediator) => await mediator.Send(new GetAllItems.Query()))
            .WithName("GetAllItems");
    }
}
