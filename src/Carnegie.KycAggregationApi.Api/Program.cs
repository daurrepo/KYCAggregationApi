using Carnegie.KycAggregationApi.Api.Middleware;
using Carnegie.KycAggregationApi.Application;
using Carnegie.KycAggregationApi.Application.Interfaces;
using Carnegie.KycAggregationApi.Infrastructure;
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

        var app = builder.Build();

        app.UseGlobalExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<KycDbContext>();
            Console.WriteLine(db.Database.GetConnectionString());
            await db.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}