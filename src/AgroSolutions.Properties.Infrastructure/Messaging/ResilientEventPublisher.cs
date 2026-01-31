using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace AgroSolutions.Properties.Infrastructure.Messaging;

/// <summary>
/// EventPublisher com circuit breaker, métricas OpenTelemetry e suporte ao Outbox pattern
/// </summary>
public class ResilientEventPublisher(
    IPublishEndpoint publishEndpoint,
    IOutboxRepository outboxRepository,
    ILogger<ResilientEventPublisher> logger
) : IEventPublisher
{
    private static readonly ActivitySource ActivitySource = new("AgroSolutions.Properties.Events");
    private static readonly Meter Meter = new("AgroSolutions.Properties.Events");
    private static readonly Counter<long> PublishedEventsCounter = Meter.CreateCounter<long>(
        "events.published",
        description: "Number of events published"
    );
    private static readonly Counter<long> FailedEventsCounter = Meter.CreateCounter<long>(
        "events.failed",
        description: "Number of events that failed to publish"
    );
    private static readonly Histogram<double> PublishDurationHistogram =
        Meter.CreateHistogram<double>(
            "events.publish.duration",
            unit: "ms",
            description: "Duration of event publishing"
        );

    private readonly ResiliencePipeline _resiliencePipeline = new ResiliencePipelineBuilder()
        .AddCircuitBreaker(
            new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    logger.LogWarning(
                        "Circuit breaker opened due to failures. Break duration: {BreakDuration}s",
                        args.BreakDuration.TotalSeconds
                    );
                    return default;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker closed");
                    return default;
                },
            }
        )
        .Build();

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class
    {
        using var activity = ActivitySource.StartActivity("PublishEvent", ActivityKind.Producer);
        activity?.SetTag("event.type", typeof(T).Name);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Tentar publicar diretamente com circuit breaker
            await _resiliencePipeline.ExecuteAsync(
                async ct =>
                {
                    await publishEndpoint.Publish(@event, ct);
                },
                cancellationToken
            );

            stopwatch.Stop();
            PublishDurationHistogram.Record(stopwatch.ElapsedMilliseconds);
            PublishedEventsCounter.Add(
                1,
                new KeyValuePair<string, object?>("event.type", typeof(T).Name)
            );

            logger.LogInformation(
                "Event {EventType} published successfully in {ElapsedMs}ms",
                typeof(T).Name,
                stopwatch.ElapsedMilliseconds
            );
        }
        catch (BrokenCircuitException ex)
        {
            stopwatch.Stop();
            FailedEventsCounter.Add(
                1,
                new KeyValuePair<string, object?>("event.type", typeof(T).Name)
            );

            logger.LogWarning(
                ex,
                "Circuit breaker is open. Saving event {EventType} to outbox",
                typeof(T).Name
            );

            // Se o circuit breaker estiver aberto, salvar na outbox
            await SaveToOutboxAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            FailedEventsCounter.Add(
                1,
                new KeyValuePair<string, object?>("event.type", typeof(T).Name)
            );

            logger.LogError(
                ex,
                "Failed to publish event {EventType}. Saving to outbox",
                typeof(T).Name
            );

            // Em caso de falha, salvar na outbox
            await SaveToOutboxAsync(@event, cancellationToken);
        }
    }

    private async Task SaveToOutboxAsync<T>(T @event, CancellationToken cancellationToken)
        where T : class
    {
        var outboxMessage = new Domain.Entities.OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(@event),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        await outboxRepository.AddAsync(outboxMessage, cancellationToken);

        logger.LogInformation(
            "Event {EventType} saved to outbox with ID {OutboxId}",
            typeof(T).Name,
            outboxMessage.Id
        );
    }
}
