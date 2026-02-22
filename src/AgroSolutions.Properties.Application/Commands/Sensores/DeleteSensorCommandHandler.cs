using AgroSolutions.Properties.Domain.Enums;
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

        var @event = new SensorEvent
        {
            FieldId = sensor.TalhaoId,
            SensorId = sensor.Id,
            DtCreated = sensor.DataInstalacao,
            TypeSensor = SensorEvent.MapTipoSensor(sensor.Tipo),
            StatusSensor = false,
            TypeOperation = TypeOperation.Delete,
        };

        await eventPublisher.PublishAsync(@event, cancellationToken);

        return Unit.Value;
    }
}
