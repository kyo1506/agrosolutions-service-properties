using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Enums;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class CreateSensorCommandHandler(
    ISensorRepository sensorRepository,
    ITalhaoRepository talhaoRepository,
    IEventPublisher eventPublisher
) : IRequestHandler<CreateSensorCommand, Guid>
{
    public async Task<Guid> Handle(CreateSensorCommand request, CancellationToken cancellationToken)
    {
        var talhao =
            await talhaoRepository.GetByIdAsync(request.TalhaoId, cancellationToken)
            ?? throw new InvalidOperationException($"Talhão {request.TalhaoId} não encontrado");

        var sensor = new Sensor
        {
            Id = Guid.NewGuid(),
            CodigoIdentificacao = request.CodigoIdentificacao,
            Tipo = Enum.Parse<TipoSensor>(request.Tipo),
            Modelo = request.Modelo,
            Fabricante = request.Fabricante,
            TalhaoId = request.TalhaoId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IntervaloLeituraMinutos = request.IntervaloLeituraMinutos ?? 15,
            DataInstalacao = DateTime.UtcNow,
            Status = StatusSensor.Ativo,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        await sensorRepository.AddAsync(sensor, cancellationToken);

        // Publicar evento de domínio para sincronizar cache dos workers
        var @event = new SensorEvent
        {
            FieldId = sensor.TalhaoId,
            SensorId = sensor.Id,
            DtCreated = sensor.DataInstalacao,
            TypeSensor = SensorEvent.MapTipoSensor(sensor.Tipo),
            StatusSensor = sensor.IsActive,
            TypeOperation = TypeOperation.Create,
        };

        await eventPublisher.PublishAsync(@event, cancellationToken);

        return sensor.Id;
    }
}
