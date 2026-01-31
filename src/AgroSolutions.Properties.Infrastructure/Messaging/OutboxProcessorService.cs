using System.Text.Json;
using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Properties.Infrastructure.Messaging;

/// <summary>
/// Background service que processa mensagens pendentes no Outbox
/// Garante entrega eventual de eventos que falharam na publicação inicial
/// </summary>
public class OutboxProcessorService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxProcessorService> logger
) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var pendingMessages = await outboxRepository.GetPendingMessagesAsync(
            100,
            cancellationToken
        );

        foreach (var message in pendingMessages)
        {
            try
            {
                // Desserializar o payload
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    logger.LogError(
                        "Could not resolve event type {EventType} for outbox message {MessageId}",
                        message.EventType,
                        message.Id
                    );
                    await outboxRepository.MarkAsFailedAsync(
                        message.Id,
                        $"Type not found: {message.EventType}",
                        cancellationToken
                    );
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Payload, eventType);
                if (@event == null)
                {
                    logger.LogError("Failed to deserialize outbox message {MessageId}", message.Id);
                    await outboxRepository.MarkAsFailedAsync(
                        message.Id,
                        "Deserialization failed",
                        cancellationToken
                    );
                    continue;
                }

                // Publicar o evento
                await publishEndpoint.Publish(@event, cancellationToken);

                // Marcar como processado
                await outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);

                logger.LogInformation(
                    "Outbox message {MessageId} of type {EventType} processed successfully",
                    message.Id,
                    message.EventType
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId} (Retry {RetryCount}/5)",
                    message.Id,
                    message.RetryCount
                );

                await outboxRepository.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }

        if (pendingMessages.Any())
        {
            logger.LogInformation("Processed {Count} outbox messages", pendingMessages.Count());
        }
    }
}
