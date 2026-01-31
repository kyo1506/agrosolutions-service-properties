using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class DeleteFazendaCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
