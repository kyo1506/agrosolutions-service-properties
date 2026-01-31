using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;

namespace AgroSolutions.Properties.Infrastructure.Messaging;

/// <summary>
/// Implementação do IEventPublisher usando MassTransit
/// </summary>
public class EventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class
    {
        await publishEndpoint.Publish(@event, cancellationToken);
    }
}
