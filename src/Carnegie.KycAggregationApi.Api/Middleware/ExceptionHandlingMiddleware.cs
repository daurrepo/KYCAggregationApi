using Carnegie.KycAggregationApi.Application.Exceptions;
using System.Text.Json;

namespace Carnegie.KycAggregationApi.Api.Middleware;

public static class ExceptionHandlingMiddleware
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

                if (exceptionFeature is null)
                {
                    return;
                }

                var ex = exceptionFeature.Error;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                var (statusCode, error) = ex switch
                {
                    CustomerNotFoundException => (StatusCodes.Status404NotFound, "Customer data not found for the provided SSN."),
                    TaxCountryNotPresentException => (StatusCodes.Status404NotFound, "Tax country information is not present for the provided customer"),
                    _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing the request."),

                };

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    logger.LogError(ex, "Unhandled exception while processing request {Method} {Path}", context.Request.Method, context.Request.Path);
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error }), context.RequestAborted);
            });
        });
    }
}
