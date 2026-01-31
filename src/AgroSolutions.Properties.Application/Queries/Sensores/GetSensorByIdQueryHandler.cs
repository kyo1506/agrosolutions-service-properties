using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Sensores;

public class GetSensorByIdQueryHandler(ISensorRepository repository, IMapper mapper)
    : IRequestHandler<GetSensorByIdQuery, SensorDto?>
{
    public async Task<SensorDto?> Handle(
        GetSensorByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var sensor = await repository.GetByIdAsync(request.Id, cancellationToken);
        return sensor == null ? null : mapper.Map<SensorDto>(sensor);
    }
}
