using System.Net;
using System.Text.Json;

namespace AgroSolutions.Properties.Api.Middlewares;

/// <summary>
/// Middleware global para tratamento de exceções
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var errorResponse = new
        {
            context.Response.StatusCode,
            Message = "An error occurred while processing your request.",
            Details = environment.IsDevelopment() ? exception.ToString() : null,
            Timestamp = DateTime.UtcNow,
            context.Request.Path,
            CorrelationId = context.Items["X-Correlation-Id"]?.ToString(),
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = environment.IsDevelopment(),
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}
