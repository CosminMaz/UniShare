using MediatR;
using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Reviews.GetAll;

public static class GetAllReviews
{
    public record Query : IRequest<IEnumerable<Review>>;

    public class Handler(UniShareContext context) : IRequestHandler<Query, IEnumerable<Review>>
    {
        public async Task<IEnumerable<Review>> Handle(Query request, CancellationToken cancellationToken)
        {
            Log.Info("All reviews were requested");
            return await context.Reviews.ToListAsync(cancellationToken);
        }
    }
}