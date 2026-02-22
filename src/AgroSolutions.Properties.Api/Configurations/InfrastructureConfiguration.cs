using AgroSolutions.Identity.Domain.Events;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Infrastructure.Data;
using AgroSolutions.Properties.Infrastructure.Messaging;
using AgroSolutions.Properties.Infrastructure.Messaging.Consumers;
using AgroSolutions.Properties.Infrastructure.Repositories;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
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

        // AWS Configuration
        var awsConfig = configuration.GetSection("AWS");
        var region = awsConfig["Region"] ?? "sa-east-1";

        // MassTransit + AWS SQS/SNS
        services.AddMassTransit(x =>
        {
            // Register Consumers
            x.AddConsumer<StatusChangedEventConsumer>();
            x.AddConsumer<UserCreatedEventConsumer>();
            x.AddConsumer<UserUpdatedEventConsumer>();
            x.AddConsumer<UserDeletedEventConsumer>();

            x.UsingAmazonSqs(
                (context, cfg) =>
                {
                    cfg.Host(
                        region,
                        h =>
                        {
                            // Credentials are loaded from environment variables or IAM roles
                            // AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, AWS_SESSION_TOKEN
                        }
                    );

                    // Mapear tipos de mensagem do Identity para o tópico SNS correto
                    // Evita que MassTransit auto-crie tópicos baseados no namespace do tipo
                    cfg.Message<UserCreatedEvent>(m =>
                        m.SetEntityName("agrosolutions-user-events")
                    );
                    cfg.Message<UserUpdatedEvent>(m =>
                        m.SetEntityName("agrosolutions-user-events")
                    );
                    cfg.Message<UserDeletedEvent>(m =>
                        m.SetEntityName("agrosolutions-user-events")
                    );

                    // Mapear eventos de propriedades para o tópico SNS correto
                    cfg.Message<SensorUpdatedEvent>(m =>
                        m.SetEntityName("agrosolutions-property-events")
                    );
                    cfg.Message<SensorDeletedEvent>(m =>
                        m.SetEntityName("agrosolutions-property-events")
                    );
                    cfg.Message<TalhaoCreatedEvent>(m =>
                        m.SetEntityName("agrosolutions-property-events")
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

                    // Configure receive endpoints for consumers
                    cfg.ReceiveEndpoint(
                        "agrosolutions-status-changed-queue",
                        e =>
                        {
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

                    cfg.ReceiveEndpoint(
                        "agrosolutions-produtor-sync-queue",
                        e =>
                        {
                            e.UseMessageRetry(r =>
                            {
                                r.Exponential(
                                    retryLimit: 3,
                                    minInterval: TimeSpan.FromSeconds(1),
                                    maxInterval: TimeSpan.FromMinutes(1),
                                    intervalDelta: TimeSpan.FromSeconds(1)
                                );
                            });

                            e.ConfigureConsumer<UserCreatedEventConsumer>(context);
                            e.ConfigureConsumer<UserUpdatedEventConsumer>(context);
                            e.ConfigureConsumer<UserDeletedEventConsumer>(context);
                        }
                    );

                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        return services;
    }
}
