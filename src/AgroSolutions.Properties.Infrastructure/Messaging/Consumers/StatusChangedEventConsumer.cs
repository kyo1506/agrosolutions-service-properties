using AgroSolutions.Properties.Domain.Enums;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Properties.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer para eventos de mudança de status vindos do worker-alerts
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
            "Received StatusChangedEvent for Talhao {TalhaoId} with status {Status}",
            message.TalhaoId,
            message.Status
        );

        try
        {
            var talhao = await talhaoRepository.GetByIdAsync(message.TalhaoId);
            if (talhao == null)
            {
                logger.LogWarning("Talhao {TalhaoId} not found", message.TalhaoId);
                return;
            }

            // Atualizar status do talhão
            talhao.Status = Enum.Parse<TalhaoStatus>(message.Status);
            talhao.Observacoes =
                $"{message.Motivo} (atualizado em {message.Timestamp:yyyy-MM-dd HH:mm:ss})";

            await talhaoRepository.UpdateAsync(talhao);

            logger.LogInformation(
                "Talhao {TalhaoId} status updated to {Status}",
                message.TalhaoId,
                message.Status
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing StatusChangedEvent for Talhao {TalhaoId}",
                message.TalhaoId
            );
            throw;
        }
    }
}
