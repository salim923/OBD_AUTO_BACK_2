using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OBD.Infrastructure.Persistence;

namespace OBD.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OBDConnection");
        services.AddDbContext<ObdDbContext>(options => options.UseSqlServer(connectionString));

    }


}
