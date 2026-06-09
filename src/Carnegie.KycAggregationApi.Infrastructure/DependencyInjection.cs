using Carnegie.KycAggregationApi.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carnegie.KycAggregationApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<KycDbContext>(options => options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IKycRepository, KycRepository>();

        return services;
    }
}
