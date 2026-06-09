using Carnegie.KycAggregationApi.Application.Interfaces;
using Carnegie.KycAggregationApi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Carnegie.KycAggregationApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IKycAggregationService, KycAggregationService>();
        return services;
    }
}
