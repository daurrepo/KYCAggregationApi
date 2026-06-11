using Carnegie.KycAggregationApi.Api.Middleware;
using Carnegie.KycAggregationApi.Application;
using Carnegie.KycAggregationApi.Application.Interfaces;
using Carnegie.KycAggregationApi.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.ConfigureHttpJsonOptions(opts =>
        {
            opts.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "KYC Aggregation API Service", Version = "v0.0.1" });
        });

        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        builder.Services.AddHttpClient<ICustomerDataClient, CustomerDataApiClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["CustomerDataApi:BaseUrl"]!);
        });

        builder.Services.AddRateLimiter(opts =>
            opts.AddFixedWindowLimiter("global", o =>
        {
            o.PermitLimit = 100;
            o.Window = TimeSpan.FromMinutes(1);
        }));

        var app = builder.Build();

        app.UseGlobalExceptionHandler();

        app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }
            await next(context);
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.DefaultModelsExpandDepth(-1));
        }
        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseRateLimiter();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KycDbContext>();
            await db.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}