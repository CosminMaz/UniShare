using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
namespace UniShare.Infrastructure.Features.Users;

public class GetAllUsersHandler
{
    private readonly UniShareContext _context;

    public GetAllUsersHandler(UniShareContext context)
    {
        _context = context;
    }

    public async Task<IResult> Handle()
    {
        var users = await _context.Users.ToListAsync();
        return Results.Ok(users);
    }
}