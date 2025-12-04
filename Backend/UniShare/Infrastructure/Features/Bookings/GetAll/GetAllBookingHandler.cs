using MediatR;
using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Bookings.GetAll;

public static class GetAllBookings
{
    public record Query : IRequest<IEnumerable<Booking>>;

    public class Handler(UniShareContext context) : IRequestHandler<Query, IEnumerable<Booking>>
    {
        public async Task<IEnumerable<Booking>> Handle(Query request, CancellationToken cancellationToken)
        {
            Log.Info("All bookings were requested");
            return await context.Bookings.ToListAsync(cancellationToken);
        }
    }
}