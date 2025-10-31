using Microsoft.EntityFrameworkCore;

namespace UniShare.Infrastructure.Persistance;

public class UniShareContext(DbContextOptions<UniShareContext> options) : DbContext(options)
{
    //It will be made with a proper dto record
    //public DbSet<Product> Products { get; set; }
}