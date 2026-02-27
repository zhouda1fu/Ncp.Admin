using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Ncp.Admin.Infrastructure;

public class DesignTimeApplicationDbContextFactory: IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c =>
            c.RegisterServicesFromAssemblies(typeof(DesignTimeApplicationDbContextFactory).Assembly));
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // change connectionstring if you want to run command "dotnet ef database update"
            options.UseNpgsql("Host=localhost;Database=dev;Username=postgres;Password=123456",
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeApplicationDbContextFactory).Assembly.FullName);
                });
        });
        var provider = services.BuildServiceProvider();
        var dbContext = provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return dbContext;
    }
}