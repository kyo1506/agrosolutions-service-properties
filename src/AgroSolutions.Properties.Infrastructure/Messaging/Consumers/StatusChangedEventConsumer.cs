using AgroSolutions.Properties.Domain.Enums;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Properties.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer MassTransit para eventos de mudança de status vindos do worker-alerts
/// </summary>
public class StatusChangedEventConsumer(
    ITalhaoRepository talhaoRepository,
    ILogger<StatusChangedEventConsumer> logger
) : IConsumer<StatusChangedEvent>
{
    public async Task Consume(ConsumeContext<StatusChangedEvent> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Processing StatusChangedEvent for Talhao {TalhaoId} with status {Status}",
            message.TalhaoId,
            message.Status
        );

        var talhao = await talhaoRepository.GetByIdAsync(
            message.TalhaoId,
            context.CancellationToken
        );
        if (talhao == null)
        {
            logger.LogWarning("Talhao {TalhaoId} not found", message.TalhaoId);
            return;
        }

        if (Enum.TryParse<TalhaoStatus>(message.Status, out var status))
        {
            talhao.Status = status;
            talhao.Observacoes =
                $"{message.Motivo} (atualizado em {message.Timestamp:yyyy-MM-dd HH:mm:ss})";

            await talhaoRepository.UpdateAsync(talhao, context.CancellationToken);

            logger.LogInformation(
                "Talhao {TalhaoId} status updated to {Status}",
                message.TalhaoId,
                message.Status
            );
        }
        else
        {
            logger.LogWarning(
                "Invalid status {Status} for Talhao {TalhaoId}",
                message.Status,
                message.TalhaoId
            );
        }
    }
}
