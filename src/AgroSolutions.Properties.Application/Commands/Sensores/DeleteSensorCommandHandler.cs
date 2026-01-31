using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class DeleteSensorCommandHandler(
    ISensorRepository sensorRepository,
    IEventPublisher eventPublisher
) : IRequestHandler<DeleteSensorCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSensorCommand request, CancellationToken cancellationToken)
    {
        var sensor =
            await sensorRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Sensor {request.Id} não encontrado");

        await sensorRepository.DeleteAsync(request.Id, cancellationToken);

        // Publicar evento para notificar outros serviços sobre a remoção
        var @event = new SensorDeletedEvent
        {
            SensorId = sensor.Id,
            CodigoIdentificacao = sensor.CodigoIdentificacao,
            TalhaoId = sensor.TalhaoId,
            FazendaId = sensor.Talhao.FazendaId,
            ProdutorId = sensor.Talhao.Fazenda.ProdutorId,
            Timestamp = DateTime.UtcNow,
        };

        await eventPublisher.PublishAsync(@event, cancellationToken);

        return Unit.Value;
    }
}
