using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Items.GetByOwner;

public static class GetItemsByOwner
{
    public record Query(Guid OwnerId) : IRequest<IEnumerable<Item>>;

    public class Handler(UniShareContext context) : IRequestHandler<Query, IEnumerable<Item>>
    {
        public async Task<IEnumerable<Item>> Handle(Query request, CancellationToken cancellationToken)
        {
            Log.Info($"Items requested for owner {request.OwnerId}");

            return await context.Items
                .Where(item => item.OwnerId == request.OwnerId)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
