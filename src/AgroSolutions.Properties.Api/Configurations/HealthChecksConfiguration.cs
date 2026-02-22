using AgroSolutions.Properties.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Properties.Api.Configurations;

public static class HealthChecksConfiguration
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString!,
                name: "postgresql",
                tags: new[] { "db", "postgresql", "ready" }
            )
            .AddDbContextCheck<PropertiesDbContext>(
                name: "dbcontext",
                tags: new[] { "db", "context", "ready" }
            );

        return services;
    }
}
