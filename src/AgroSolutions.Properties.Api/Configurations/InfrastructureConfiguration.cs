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
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Event Publisher with resilience (circuit breaker + outbox)
        services.AddScoped<IEventPublisher, ResilientEventPublisher>();

        // Outbox Processor Background Service
        services.AddHostedService<OutboxProcessorService>();

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

                    // Retry policy com exponential backoff
                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(2),
                            maxInterval: TimeSpan.FromMinutes(5),
                            intervalDelta: TimeSpan.FromSeconds(2)
                        );
                    });

                    // Configurar fila para consumir eventos de status do worker-alerts
                    cfg.ReceiveEndpoint(
                        "status-changed-queue",
                        e =>
                        {
                            // Dead Letter Queue configuration
                            e.UseMessageRetry(r =>
                            {
                                r.Exponential(
                                    retryLimit: 3,
                                    minInterval: TimeSpan.FromSeconds(1),
                                    maxInterval: TimeSpan.FromMinutes(1),
                                    intervalDelta: TimeSpan.FromSeconds(1)
                                );
                            });

                            e.ConfigureConsumer<StatusChangedEventConsumer>(context);
                        }
                    );

                    // Configurar fila para consumir eventos de produtores do Identity
                    cfg.ReceiveEndpoint(
                        "produtor-sync-queue",
                        e =>
                        {
                            // Dead Letter Queue configuration
                            e.UseMessageRetry(r =>
                            {
                                r.Exponential(
                                    retryLimit: 3,
                                    minInterval: TimeSpan.FromSeconds(1),
                                    maxInterval: TimeSpan.FromMinutes(1),
                                    intervalDelta: TimeSpan.FromSeconds(1)
                                );
                            });

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
