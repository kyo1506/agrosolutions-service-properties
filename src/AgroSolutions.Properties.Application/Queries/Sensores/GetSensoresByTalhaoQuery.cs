using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Sensores;

public class GetSensoresByTalhaoQuery : IRequest<IEnumerable<SensorDto>>
{
    public Guid TalhaoId { get; set; }
}
