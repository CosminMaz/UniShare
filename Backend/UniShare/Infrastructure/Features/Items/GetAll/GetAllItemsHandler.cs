using MediatR;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
using UniShare.Common;

namespace UniShare.Infrastructure.Features.Items.GetAll;

public static class GetAllItems
{
    public record Query : IRequest<IEnumerable<Item>>;

    public class Handler(UniShareContext context) : IRequestHandler<Query, IEnumerable<Item>>
    {
        public async Task<IEnumerable<Item>> Handle(Query request, CancellationToken cancellationToken)
        {
            Log.Info("All items were requested");
            return await context.Items.ToListAsync(cancellationToken);
        }
    }
}
