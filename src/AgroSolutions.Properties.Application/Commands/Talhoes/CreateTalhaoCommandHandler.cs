using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Enums;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class CreateTalhaoCommandHandler(
    ITalhaoRepository talhaoRepository,
    IFazendaRepository fazendaRepository,
    IEventPublisher eventPublisher
) : IRequestHandler<CreateTalhaoCommand, Guid>
{
    public async Task<Guid> Handle(CreateTalhaoCommand request, CancellationToken cancellationToken)
    {
        // Validar se fazenda existe
        var fazenda =
            await fazendaRepository.GetByIdAsync(request.FazendaId, cancellationToken)
            ?? throw new InvalidOperationException($"Fazenda {request.FazendaId} não encontrada");

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
        if (request.Sensores?.Any() == true)
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

                // Publicar evento de sensor criado
                await eventPublisher.PublishAsync(
                    new SensorUpdatedEvent
                    {
                        SensorId = sensor.Id,
                        CodigoIdentificacao = sensor.CodigoIdentificacao,
                        TalhaoId = sensor.TalhaoId,
                        FazendaId = fazenda.Id,
                        ProdutorId = fazenda.ProdutorId,
                        TipoSensor = sensor.Tipo.ToString(),
                        IsActive = true,
                        Timestamp = DateTime.UtcNow,
                    },
                    cancellationToken
                );
            }
        }

        await talhaoRepository.AddAsync(talhao, cancellationToken);

        // Publicar evento de talhão criado
        await eventPublisher.PublishAsync(
            new TalhaoCreatedEvent
            {
                TalhaoId = talhao.Id,
                Nome = talhao.Nome,
                FazendaId = fazenda.Id,
                ProdutorId = fazenda.ProdutorId,
                Timestamp = DateTime.UtcNow,
            },
            cancellationToken
        );

        return talhao.Id;
    }
}
