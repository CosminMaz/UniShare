using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniShare.Infrastructure.Features.Bookings.CreateBooking;
using UniShare.Infrastructure.Features.Bookings.GetAll;
using UniShare.Infrastructure.Features.Bookings.ApproveBooking;
using UniShare.Infrastructure.Features.Bookings.CompleteBooking;

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

        app.MapPost("/bookings/{bookingId}/approve",
                async (Guid bookingId,
                        [FromBody] ApproveBookingBody body,
                        [FromServices] ApproveBookingHandler handler) =>
                {
                    var approveRequest = new ApproveBookingRequest(bookingId, body.Approve);
                    return await handler.Handle(approveRequest);
                })
            .WithName("ApproveBooking")
            .Accepts<ApproveBookingBody>("application/json")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        app.MapPost("/bookings/{bookingId}/reject",
                async (Guid bookingId,
                        [FromServices] ApproveBookingHandler handler) =>
                {
                    var rejectRequest = new ApproveBookingRequest(bookingId, false);
                    return await handler.Handle(rejectRequest);
                })
            .WithName("RejectBooking")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        app.MapPost("/bookings/{bookingId}/complete",
                async (Guid bookingId, [FromServices] CompleteBookingHandler handler) =>
                    await handler.Handle(bookingId))
            .WithName("CompleteBooking")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);
    }
}