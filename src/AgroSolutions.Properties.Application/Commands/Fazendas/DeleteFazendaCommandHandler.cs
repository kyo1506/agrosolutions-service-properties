using AgroSolutions.Properties.Domain.Interfaces;
using MediatR;

namespace AgroSolutions.Properties.Application.Commands.Fazendas;

public class DeleteFazendaCommandHandler(IFazendaRepository fazendaRepository)
    : IRequestHandler<DeleteFazendaCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteFazendaCommand request,
        CancellationToken cancellationToken
    )
    {
        await fazendaRepository.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
