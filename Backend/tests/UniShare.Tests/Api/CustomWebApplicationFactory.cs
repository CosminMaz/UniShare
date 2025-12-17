using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniShare.Infrastructure.Persistence;

namespace UniShare.Tests.Api;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<UniShareContext>) || 
                d.ServiceType == typeof(UniShareContext)).ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<UniShareContext>(options =>
            {
                options.UseInMemoryDatabase("UserApiTestsDb");
            });
        });
    }
}