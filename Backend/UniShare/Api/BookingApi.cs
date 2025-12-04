using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniShare.Infrastructure.Features.Bookings.CreateBooking;
using UniShare.Infrastructure.Features.Bookings.GetAll;

namespace UniShare.Api;

public static class BookingApi
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/bookings",
                async ([FromBody] CreateBookingRequest request,
                        [FromServices] CreateBookingHandler handler) =>
                    await handler.Handle(request))
            .WithName("CreateBooking")
            .Accepts<CreateBookingRequest>("application/json")
            .Produces(201)
            .Produces(400)
            .Produces(401);

        app.MapGet("/bookings",
                async (IMediator mediator) =>
                    await mediator.Send(new GetAllBookings.Query()))
            .WithName("GetAllBookings");
    }
}