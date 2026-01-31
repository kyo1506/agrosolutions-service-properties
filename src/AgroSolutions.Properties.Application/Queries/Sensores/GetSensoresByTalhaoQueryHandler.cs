using AgroSolutions.Properties.Domain.Interfaces;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Sensores;

public class GetSensoresByTalhaoQueryHandler(ISensorRepository repository, IMapper mapper)
    : IRequestHandler<GetSensoresByTalhaoQuery, IEnumerable<SensorDto>>
{
    public async Task<IEnumerable<SensorDto>> Handle(
        GetSensoresByTalhaoQuery request,
        CancellationToken cancellationToken
    )
    {
        var sensores = await repository.GetByTalhaoIdAsync(request.TalhaoId, cancellationToken);
        return mapper.Map<IEnumerable<SensorDto>>(sensores);
    }
}
