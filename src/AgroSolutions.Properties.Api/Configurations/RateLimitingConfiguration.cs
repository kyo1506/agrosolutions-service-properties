using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AgroSolutions.Properties.Api.Configurations;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitingConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var rateLimitSettings = configuration.GetSection("RateLimiting");
        var enableRateLimiting = rateLimitSettings.GetValue("EnableRateLimiting", true);

        if (!enableRateLimiting)
        {
            return services;
        }

        services.AddRateLimiter(options =>
        {
            // Reject requests when rate limit is exceeded
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Default policy: Fixed window per Client ID
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                // Extract Client ID from header
                var clientId =
                    context.Request.Headers["X-Client-Id"].FirstOrDefault()
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitSettings.GetValue("DefaultLimit", 100),
                        Window = TimeSpan.FromSeconds(
                            rateLimitSettings.GetValue("DefaultPeriodInSeconds", 60)
                        ),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }
                );
            });

            // Policy for public endpoints (more restrictive)
            options.AddPolicy(
                "PublicApi",
                context =>
                {
                    var clientId =
                        context.Request.Headers["X-Client-Id"].FirstOrDefault()
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientId,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitSettings.GetValue("PublicLimit", 50),
                            Window = TimeSpan.FromSeconds(
                                rateLimitSettings.GetValue("PublicPeriodInSeconds", 60)
                            ),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0,
                        }
                    );
                }
            );

            // Policy for authenticated endpoints (less restrictive)
            options.AddPolicy(
                "AuthenticatedApi",
                context =>
                {
                    var clientId =
                        context.Request.Headers["X-Client-Id"].FirstOrDefault()
                        ?? context.User?.Identity?.Name
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientId,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimitSettings.GetValue("AuthenticatedLimit", 200),
                            Window = TimeSpan.FromSeconds(
                                rateLimitSettings.GetValue("AuthenticatedPeriodInSeconds", 60)
                            ),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0,
                        }
                    );
                }
            );

            // On rejection, add headers to response
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = (
                        (int)retryAfter.TotalSeconds
                    ).ToString();

                    await context.HttpContext.Response.WriteAsync(
                        $"Rate limit exceeded. Retry after {retryAfter.TotalSeconds} seconds.",
                        cancellationToken
                    );
                }
                else
                {
                    context.HttpContext.Response.Headers.RetryAfter = "60";
                    await context.HttpContext.Response.WriteAsync(
                        "Rate limit exceeded. Please try again later.",
                        cancellationToken
                    );
                }
            };
        });

        return services;
    }
}
