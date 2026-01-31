using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Sensores;

public class DeleteSensorCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
