using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Domain.Enums;
using AgroSolutions.Properties.Domain.Events;
using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class CreateFazendaCommandHandler(
    IFazendaRepository fazendaRepository,
    IProdutorRepository produtorRepository,
    IEventPublisher eventPublisher
) : IRequestHandler<CreateFazendaCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateFazendaCommand request,
        CancellationToken cancellationToken
    )
    {
        // Validar se produtor existe
        var produtor =
            await produtorRepository.GetByIdAsync(request.ProdutorId, cancellationToken)
            ?? throw new InvalidOperationException($"Produtor {request.ProdutorId} não encontrado");

        var fazenda = new Fazenda
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            AreaTotal = request.AreaTotal,
            Localizacao = request.Localizacao,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Cidade = request.Cidade,
            Estado = request.Estado,
            ProdutorId = request.ProdutorId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        // Criar talhões se fornecidos
        if (request.Talhoes?.Any() == true)
        {
            foreach (var talhaoDto in request.Talhoes)
            {
                var talhao = new Talhao
                {
                    Id = Guid.NewGuid(),
                    Nome = talhaoDto.Nome,
                    Area = talhaoDto.Area,
                    Cultura = talhaoDto.Cultura,
                    DataPlantio = talhaoDto.DataPlantio,
                    Observacoes = talhaoDto.Observacoes,
                    Status = TalhaoStatus.Normal,
                    FazendaId = fazenda.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                };

                // Criar sensores se fornecidos
                if (talhaoDto.Sensores?.Any() == true)
                {
                    foreach (var sensorDto in talhaoDto.Sensores)
                    {
                        if (!Enum.TryParse<TipoSensor>(sensorDto.Tipo, out var tipoSensor))
                        {
                            throw new ArgumentException(
                                $"Tipo de sensor inválido: {sensorDto.Tipo}"
                            );
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
                                ProdutorId = produtor.Id,
                                TipoSensor = sensor.Tipo.ToString(),
                                IsActive = true,
                                Timestamp = DateTime.UtcNow,
                            },
                            cancellationToken
                        );
                    }
                }

                fazenda.Talhoes.Add(talhao);

                // Publicar evento de talhão criado
                await eventPublisher.PublishAsync(
                    new TalhaoCreatedEvent
                    {
                        TalhaoId = talhao.Id,
                        Nome = talhao.Nome,
                        FazendaId = fazenda.Id,
                        ProdutorId = produtor.Id,
                        Timestamp = DateTime.UtcNow,
                    },
                    cancellationToken
                );
            }
        }

        await fazendaRepository.AddAsync(fazenda, cancellationToken);

        return fazenda.Id;
    }
}
