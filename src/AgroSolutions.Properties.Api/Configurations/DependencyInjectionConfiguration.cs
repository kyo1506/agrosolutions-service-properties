using AgroSolutions.Properties.Application.Mappings;
using FluentValidation;

namespace AgroSolutions.Properties.Api.Configurations;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(MappingProfile).Assembly)
        );

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

        return services;
    }
}
