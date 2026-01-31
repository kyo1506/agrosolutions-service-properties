using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Talhoes;

public class DeleteTalhaoCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
