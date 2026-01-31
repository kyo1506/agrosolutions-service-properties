using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Infrastructure.Data;
using AgroSolutions.Properties.Infrastructure.Messaging;
using AgroSolutions.Properties.Infrastructure.Messaging.Consumers;
using AgroSolutions.Properties.Infrastructure.Repositories;
using MassTransit;

namespace AgroSolutions.Properties.Api.Configurations;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Repositories
        services.AddScoped<IProdutorRepository, ProdutorRepository>();
        services.AddScoped<IFazendaRepository, FazendaRepository>();
        services.AddScoped<ITalhaoRepository, TalhaoRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();

        // Event Publisher
        services.AddScoped<IEventPublisher, EventPublisher>();

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            // Consumers
            x.AddConsumer<StatusChangedEventConsumer>();
            x.AddConsumer<ProdutorEventsConsumer>();

            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    var rabbitMqSettings = configuration.GetSection("RabbitMQ");

                    cfg.Host(
                        rabbitMqSettings["Host"],
                        "/",
                        h =>
                        {
                            h.Username(rabbitMqSettings["Username"] ?? "guest");
                            h.Password(rabbitMqSettings["Password"] ?? "guest");
                        }
                    );

                    // Configurar fila para consumir eventos de status do worker-alerts
                    cfg.ReceiveEndpoint(
                        "status-changed-queue",
                        e =>
                        {
                            e.ConfigureConsumer<StatusChangedEventConsumer>(context);
                        }
                    );

                    // Configurar fila para consumir eventos de produtores do Identity
                    cfg.ReceiveEndpoint(
                        "produtor-sync-queue",
                        e =>
                        {
                            e.ConfigureConsumer<ProdutorEventsConsumer>(context);
                        }
                    );

                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        return services;
    }
}
