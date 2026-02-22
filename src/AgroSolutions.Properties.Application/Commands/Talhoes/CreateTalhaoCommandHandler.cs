using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Enums;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class CreateTalhaoCommandHandler(
    ITalhaoRepository talhaoRepository,
    IEventPublisher eventPublisher
) : IRequestHandler<CreateTalhaoCommand, Guid>
{
    public async Task<Guid> Handle(CreateTalhaoCommand request, CancellationToken cancellationToken)
    {
        var talhao = new Talhao
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Area = request.Area,
            Cultura = request.Cultura,
            DataPlantio = request.DataPlantio.HasValue
                ? DateTime.SpecifyKind(request.DataPlantio.Value, DateTimeKind.Utc)
                : null,
            Observacoes = request.Observacoes,
            Status = TalhaoStatus.Normal,
            FazendaId = request.FazendaId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        // Criar sensores se fornecidos
        if (request.Sensores?.Count > 0)
        {
            foreach (var sensorDto in request.Sensores)
            {
                if (!Enum.TryParse<TipoSensor>(sensorDto.Tipo, out var tipoSensor))
                {
                    throw new ArgumentException($"Tipo de sensor inválido: {sensorDto.Tipo}");
                }

                var sensor = new Sensor
                {
                    Id = Guid.NewGuid(),
                    CodigoIdentificacao = sensorDto.CodigoIdentificacao,
                    Tipo = tipoSensor,
                    Modelo = sensorDto.Modelo,
                    Fabricante = sensorDto.Fabricante,
                    Latitude = sensorDto.Latitude,
                    Longitude = sensorDto.Longitude,
                    IntervaloLeituraMinutos = sensorDto.IntervaloLeituraMinutos ?? 15,
                    DataInstalacao = DateTime.UtcNow,
                    Status = StatusSensor.Ativo,
                    TalhaoId = talhao.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                };

                talhao.Sensores.Add(sensor);

                await eventPublisher.PublishAsync(
                    new SensorEvent
                    {
                        FieldId = sensor.TalhaoId,
                        SensorId = sensor.Id,
                        DtCreated = sensor.DataInstalacao,
                        TypeSensor = SensorEvent.MapTipoSensor(sensor.Tipo),
                        StatusSensor = true,
                        TypeOperation = TypeOperation.Create,
                    },
                    cancellationToken
                );
            }
        }

        await talhaoRepository.AddAsync(talhao, cancellationToken);

        return talhao.Id;
    }
}
