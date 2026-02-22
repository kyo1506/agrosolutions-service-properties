using AgroSolutions.Properties.Domain.Enums;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class UpdateSensorCommandHandler(
    ISensorRepository sensorRepository,
    IEventPublisher eventPublisher
) : IRequestHandler<UpdateSensorCommand, Unit>
{
    public async Task<Unit> Handle(UpdateSensorCommand request, CancellationToken cancellationToken)
    {
        var sensor =
            await sensorRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Sensor {request.Id} não encontrado");

        sensor.Modelo = request.Modelo;
        sensor.Fabricante = request.Fabricante;
        sensor.Latitude = request.Latitude;
        sensor.Longitude = request.Longitude;
        sensor.IntervaloLeituraMinutos = request.IntervaloLeituraMinutos;

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<StatusSensor>(request.Status, out var statusSensor))
            {
                sensor.Status = statusSensor;
            }
        }

        sensor.UpdatedAt = DateTime.UtcNow;

        await sensorRepository.UpdateAsync(sensor, cancellationToken);

        // Publicar evento de atualização
        await eventPublisher.PublishAsync(
            new SensorEvent
            {
                FieldId = sensor.TalhaoId,
                SensorId = sensor.Id,
                DtCreated = sensor.DataInstalacao,
                TypeSensor = SensorEvent.MapTipoSensor(sensor.Tipo),
                StatusSensor = sensor.IsActive,
                TypeOperation = TypeOperation.Update,
            },
            cancellationToken
        );

        return Unit.Value;
    }
}
