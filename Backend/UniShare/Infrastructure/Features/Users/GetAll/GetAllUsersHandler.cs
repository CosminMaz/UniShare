using Microsoft.EntityFrameworkCore;
using UniShare.Common;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Infrastructure.Features.Users.GetAll;

public class GetAllUsersHandler(UniShareContext context)
{
    public async Task<IResult> Handle()
    {
        var users = await context.Users.ToListAsync();
        Log.Info("All users were requested");
        return Results.Ok(users);
    }
}
