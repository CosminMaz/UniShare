using MediatR;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Items.GetAll;

public static class GetAllItems
{
    public record Query : IRequest<IEnumerable<Item>>;

    public class Handler : IRequestHandler<Query, IEnumerable<Item>>
    {
        private readonly UniShareContext _context;

        public Handler(UniShareContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.Items.ToListAsync(cancellationToken);
        }
    }
}