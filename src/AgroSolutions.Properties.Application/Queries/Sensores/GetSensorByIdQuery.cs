using AgroSolutions.Properties.Shared.DTOs;
using MediatR;

namespace AgroSolutions.Properties.Application.Queries.Sensores;

public class GetSensorByIdQuery : IRequest<SensorDto?>
{
    public Guid Id { get; set; }
}
