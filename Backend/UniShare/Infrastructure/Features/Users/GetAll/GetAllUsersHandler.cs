using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Persistence;
namespace UniShare.Infrastructure.Features.Users;

public class GetAllUsersHandler(UniShareContext context)
{
    public async Task<IResult> Handle()
    {
        var users = await context.Users.ToListAsync();
        return Results.Ok(users);
    }
}